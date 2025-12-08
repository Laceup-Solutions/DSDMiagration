using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class BatchDepartmentPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private Client? _client;
        private Batch? _batch;
        private bool _initialized;

        public ObservableCollection<ClientDepartment> Departments { get; } = new();
        public ObservableCollection<BatchDepartmentOrderViewModel> Orders { get; } = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private ClientDepartment? _selectedDepartment;

        [ObservableProperty]
        private bool _canSelectDepartment = true;

        [ObservableProperty]
        private bool _canAddItems;

        [ObservableProperty]
        private bool _canPrint;

        [ObservableProperty]
        private bool _canDelete;

        [ObservableProperty]
        private bool _canSend;

        [ObservableProperty]
        private bool _showDelete = true;

        public BatchDepartmentPageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;

            Orders.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (BatchDepartmentOrderViewModel order in e.NewItems)
                    {
                        order.PropertyChanged += (sender, args) =>
                        {
                            if (args.PropertyName == nameof(BatchDepartmentOrderViewModel.IsSelected))
                            {
                                UpdateButtonStates();
                            }
                        };
                    }
                }
            };
        }

        public async Task InitializeAsync(int clientId, int batchId)
        {
            if (_initialized && _client?.ClientId == clientId && _batch?.Id == batchId)
            {
                await RefreshAsync();
                return;
            }

            _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);
            _batch = Batch.List.FirstOrDefault(x => x.Id == batchId);

            if (_client == null || _batch == null)
            {
                await _dialogService.ShowAlertAsync("Client or batch not found.", "Error");
                return;
            }

            _initialized = true;
            LoadData();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            LoadData();
            await Task.CompletedTask;
        }

        private void LoadData()
        {
            if (_client == null || _batch == null)
                return;

            ClientName = _client.ClientName;

            // Load departments
            var departments = ClientDepartment.GetDepartmentsForClient(_client);
            Departments.Clear();
            foreach (var dept in departments)
            {
                Departments.Add(dept);
            }

            // Load orders
            var orders = _batch.Orders().ToList();
            Orders.Clear();

            foreach (var order in orders)
            {
                var orderViewModel = CreateOrderViewModel(order);
                Orders.Add(orderViewModel);
            }

            UpdateButtonStates();
            ShowDelete = !_client.OneOrderPerDepartment;
        }

        private BatchDepartmentOrderViewModel CreateOrderViewModel(Order order)
        {
            var departmentName = order.Department != null ? order.Department.Name : "No Department";
            var totalText = $"Total: {order.OrderTotalCost().ToCustomString()}";

            string statusText = string.Empty;
            var statusColor = Colors.Transparent;

            if (order.Voided)
            {
                statusText = "Voided";
                statusColor = Colors.Red;
            }
            else if (order.Finished)
            {
                statusText = "Finalized";
                statusColor = Colors.Green;
            }
            else if (order.Dexed)
            {
                statusText = "DEX Sent";
                statusColor = Colors.Blue;
            }

            return new BatchDepartmentOrderViewModel
            {
                Order = order,
                DepartmentName = departmentName,
                TotalText = totalText,
                StatusText = statusText,
                StatusColor = statusColor,
                IsSelected = false
            };
        }

        private void UpdateButtonStates()
        {
            var selectedOrders = Orders.Where(x => x.IsSelected).Select(x => x.Order).ToList();
            var allOrders = Orders.Select(x => x.Order).ToList();

            CanAddItems = SelectedDepartment != null;
            CanPrint = allOrders.Count > 0;
            CanDelete = selectedOrders.Count > 0 && ShowDelete;
            CanSend = allOrders.Count > 0;
        }

        partial void OnSelectedDepartmentChanged(ClientDepartment? value)
        {
            UpdateButtonStates();
        }

        [RelayCommand]
        private async Task AddItemsAsync()
        {
            if (_batch == null || _client == null || SelectedDepartment == null)
                return;

            // Find or create order for this department
            var order = _batch.Orders().FirstOrDefault(x => 
                x.DepartmentId == SelectedDepartment.DepartmentId);

            if (order == null)
            {
                order = new Order(_client) { OrderType = OrderType.Order };
                order.BatchId = _batch.Id;
                order.DepartmentId = SelectedDepartment.DepartmentId;
                order.AsPresale = true;
                order.Save();
            }

            // Navigate to order details
            if (Config.UseFullTemplateForClient(_client) && !_client.AllowOneDoc)
            {
                await Shell.Current.GoToAsync($"superordertemplate?orderId={order.OrderId}&asPresale=1");
            }
            else
            {
                await Shell.Current.GoToAsync($"orderdetails?orderId={order.OrderId}&asPresale=1");
            }
        }

        [RelayCommand]
        private async Task PrintAsync()
        {
            if (_batch == null || _client == null)
                return;

            List<Order> ordersToPrint;

            if (_client.OneOrderPerDepartment)
            {
                // Merge all orders
                var allOrders = _batch.Orders().ToList();
                if (allOrders.Count == 0)
                {
                    await _dialogService.ShowAlertAsync("No orders to print.", "Alert");
                    return;
                }

                ordersToPrint = new List<Order> { MergeOrders(allOrders) };
            }
            else
            {
                var selectedOrders = Orders.Where(x => x.IsSelected).Select(x => x.Order).ToList();
                if (selectedOrders.Count == 0)
                {
                    await _dialogService.ShowAlertAsync("Please select orders to print.", "Alert");
                    return;
                }

                ordersToPrint = selectedOrders;
            }

            var copies = await _dialogService.ShowPromptAsync("Copies to Print", "Enter number of copies:", initialValue: "1", keyboard: Keyboard.Numeric);
            if (string.IsNullOrWhiteSpace(copies) || !int.TryParse(copies, out var copiesCount) || copiesCount < 1)
            {
                await _dialogService.ShowAlertAsync("Please enter a valid number of copies.", "Alert");
                return;
            }

            // TODO: Implement printing
            await _dialogService.ShowAlertAsync("Print functionality is not yet fully implemented in the MAUI version.", "Info");
        }

        [RelayCommand]
        private async Task DeleteAsync()
        {
            if (_batch == null)
                return;

            var selectedOrders = Orders.Where(x => x.IsSelected).Select(x => x.Order).ToList();
            if (selectedOrders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select orders to delete.", "Alert");
                return;
            }

            var result = await _dialogService.ShowConfirmAsync(
                $"Delete {selectedOrders.Count} order(s)?",
                "Warning",
                "Yes",
                "No");

            if (result)
            {
                foreach (var order in selectedOrders)
                {
                    ClientDailyParLevel.Void(order.Client.ClientId, DateTime.Now.DayOfWeek, order.DepartmentUniqueId);
                    order.Delete();
                }

                LoadData();
            }
        }

        [RelayCommand]
        private async Task SendAsync()
        {
            if (_batch == null)
                return;

            var orders = _batch.Orders().ToList();
            if (orders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No orders to send.", "Alert");
                return;
            }

            await SendOrderAsync();
        }

        private async Task SendOrderAsync()
        {
            if (_batch == null)
                return;

            var orders = _batch.Orders().ToList();
            if (orders.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No orders to send.", "Alert");
                return;
            }

            try
            {
                await _dialogService.ShowLoadingAsync("Sending orders...");

                // Clock out batch if needed
                if (_batch.ClockedOut == DateTime.MinValue)
                {
                    _batch.ClockedOut = DateTime.Now;
                    _batch.Save();
                }

                List<string> ordersIds = null;

                // Get history from orders
                // TODO: Implement GetHistoryFromOrders if needed

                if (_client.OneOrderPerDepartment)
                {
                    if (Config.GeneratePresaleNumber)
                    {
                        var firstOrder = orders.FirstOrDefault();
                        if (firstOrder != null && string.IsNullOrEmpty(firstOrder.PrintedOrderId))
                        {
                            firstOrder.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(firstOrder);
                            firstOrder.Save();
                        }
                    }
                }
                else
                {
                    // Get selected orders - in MAUI, we might need to track selection differently
                    // For now, send all orders
                    ordersIds = orders.Select(x => x.OrderId.ToString()).ToList();
                }

                // Send client departments if updated
                if (ClientDepartment.Departments.Any(x => x.Updated == true))
                    DataAccess.SendClientDepartments(false);

                // Send the orders
                DataAccess.SendTheOrders(new Batch[] { _batch }, ordersIds, true, true);

                await _dialogService.HideLoadingAsync();
                await _dialogService.ShowAlertAsync("Orders sent successfully.", "Success");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error sending orders.", "Alert");
            }
        }

        [RelayCommand]
        private async Task DoneAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        private Order MergeOrders(System.Collections.Generic.List<Order> orders)
        {
            var firstOrder = orders.FirstOrDefault();
            if (firstOrder == null)
                return null;

            var result = Order.DuplicateorderHeader(firstOrder);
            foreach (var order in orders)
            {
                foreach (var item in order.Details)
                {
                    item.ProductDepartment = order.Department != null ? order.Department.Name : "";
                    result.Details.Add(item);
                }
            }

            return result;
        }

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            if (_client == null || _batch == null)
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

        private List<MenuOption> BuildMenuOptions()
        {
            var options = new List<MenuOption>();

            if (_client == null || _batch == null)
                return options;

            // Add Comments (only if OneOrderPerDepartment)
            if (_client.OneOrderPerDepartment)
            {
                options.Add(new MenuOption("Add Comments", async () =>
                {
                    var comments = await _dialogService.ShowPromptAsync("Add Comments", "Enter comments:", initialValue: string.Empty);
                    if (comments != null)
                    {
                        var firstOrder = _batch.Orders().FirstOrDefault();
                        if (firstOrder != null)
                        {
                            firstOrder.Comments = comments;
                            firstOrder.Save();
                            await _dialogService.ShowAlertAsync("Comments saved.", "Success");
                        }
                    }
                }));
            }

            // Set PO (only if OneOrderPerDepartment and Config.SetPO)
            if (_client.OneOrderPerDepartment && Config.SetPO)
            {
                options.Add(new MenuOption("Set PO", async () =>
                {
                    var firstOrder = _batch.Orders().FirstOrDefault();
                    var po = await _dialogService.ShowPromptAsync("Set PO", "Enter PO Number:", initialValue: firstOrder?.PONumber ?? string.Empty);
                    if (!string.IsNullOrWhiteSpace(po) && firstOrder != null)
                    {
                        firstOrder.PONumber = po;
                        firstOrder.Save();
                        await _dialogService.ShowAlertAsync("PO number set.", "Success");
                    }
                }));
            }

            // Set Ship Date (only if OneOrderPerDepartment)
            if (_client.OneOrderPerDepartment)
            {
                options.Add(new MenuOption("Set Ship Date", async () =>
                {
                    var orders = Order.Orders.Where(x => x.Client.ClientId == _client.ClientId && !x.Finished).ToList();
                    if (orders.Count == 0)
                    {
                        await _dialogService.ShowAlertAsync("No active orders found for this client.", "Info");
                        return;
                    }

                    var currentShipDate = orders.First().ShipDate.Year == 1 ? DateTime.Now : orders.First().ShipDate;
                    var selectedDate = await _dialogService.ShowDatePickerAsync("Set Ship Date", currentShipDate, DateTime.Now, null);
                    if (selectedDate.HasValue)
                    {
                        foreach (var order in orders)
                        {
                            order.ShipDate = selectedDate.Value;
                            order.Save();
                        }
                        await _dialogService.ShowAlertAsync($"Ship date set for {orders.Count} order(s).", "Success");
                    }
                }));
            }

            // Send by Email
            options.Add(new MenuOption("Send by Email", async () =>
            {
                await SendByEmailAsync();
            }));

            // Advanced Options
            options.Add(new MenuOption("Advanced Options", ShowAdvancedOptionsAsync));

            return options;
        }

        private async Task SendByEmailAsync()
        {
            try
            {
                var selectedOrders = Orders.Where(x => x.IsSelected).Select(x => x.Order).ToList();
                
                if (selectedOrders.Count == 0)
                {
                    // If no orders selected, send all orders merged (matches Xamarin BatchDepartmentActivity)
                    var allOrders = Orders.Select(x => x.Order).ToList();
                    if (allOrders.Count == 0)
                    {
                        await _dialogService.ShowAlertAsync("No orders to send.", "Alert", "OK");
                        return;
                    }
                    
                    // Merge orders into one (matches Xamarin MergeOrders logic)
                    var mergedOrder = allOrders.First();
                    if (allOrders.Count > 1)
                    {
                        // Merge details from other orders
                        foreach (var order in allOrders.Skip(1))
                        {
                            foreach (var detail in order.Details)
                            {
                                mergedOrder.Details.Add(detail);
                            }
                        }
                    }
                    
                    await PdfHelper.SendOrderByEmail(mergedOrder);
                }
                else
                {
                    await PdfHelper.SendOrdersByEmail(selectedOrders);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
            }
        }

        private async Task ShowAdvancedOptionsAsync()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
        }
    }

    public partial class BatchDepartmentOrderViewModel : ObservableObject
    {
        [ObservableProperty]
        private bool _isSelected;

        [ObservableProperty]
        private string _departmentName = string.Empty;

        [ObservableProperty]
        private string _totalText = string.Empty;

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private Microsoft.Maui.Graphics.Color _statusColor;

        public Order Order { get; set; } = null!;
    }
}

