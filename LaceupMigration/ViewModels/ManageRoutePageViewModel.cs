using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class ManageRoutePageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private string _selectedDriver = "Not Selected";
        [ObservableProperty] private string _selectedSalesman = "Not Selected";
        [ObservableProperty] private string _selectedDate = string.Empty;
        [ObservableProperty] private ObservableCollection<RouteItemViewModel> _routeItems = new();
        [ObservableProperty] private int _selectedDriverId;
        [ObservableProperty] private int _selectedSalesmanId;
        [ObservableProperty] private DateTime _routeDate = DateTime.Today;
        private DriverRoute? _currentRoute;
        private string _routeFileName = string.Empty;

        public ManageRoutePageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            SelectedDate = DateTime.Today.ToShortDateString();
            _routeFileName = Path.Combine(Config.DataPath, "manageRoute.xml");
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                // Load existing route if available
                _currentRoute = DriverRoute.Load(_routeFileName);
                
                if (_currentRoute != null)
                {
                    RouteDate = _currentRoute.Date;
                    SelectedDate = RouteDate.ToShortDateString();
                    
                    // Determine if driver or salesman
                    var trucksWithDriver = Truck.Trucks.Where(x => x.DriverId > 0).ToList();
                    var driver = Salesman.List.FirstOrDefault(x => x.Id == _currentRoute.DriverId);
                    var truck = trucksWithDriver.FirstOrDefault(x => x.DriverId == _currentRoute.DriverId);
                    
                    if (truck != null)
                    {
                        SelectedDriver = truck.Name;
                        SelectedDriverId = truck.DriverId ?? 0;
                    }
                    else if (driver != null)
                    {
                        SelectedSalesman = driver.Name;
                        SelectedSalesmanId = driver.Id;
                    }

                    // Load route items
                    RouteItems.Clear();
                    foreach (var detail in _currentRoute.Details.Where(x => !x.Deleted).OrderBy(x => x.Stop))
                    {
                        Client? client = null;
                        if (detail.ClientId > 0)
                        {
                            client = Client.Find(detail.ClientId);
                        }
                        else if (detail.Order != null)
                        {
                            client = Client.Find(detail.Order.ClientId);
                        }

                        if (client != null)
                        {
                            RouteItems.Add(new RouteItemViewModel
                            {
                                ClientId = client.ClientId,
                                ClientName = client.ClientName ?? "Unknown",
                                Address = ParseAddress(client.ShipToAddress),
                                Sequence = detail.Stop,
                                OrderId = detail.Order?.Id ?? 0
                            });
                        }
                    }
                }
                else
                {
                    RouteItems.Clear();
                }
                
                UpdateSequenceNumbers();
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading route: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        public void OnNavigatedTo(IDictionary<string, object> query)
        {
            if (query != null)
            {
                // Handle results from AddOrdersToRoutePage
                if (query.TryGetValue("selectedOrderIds", out var orderIds) && orderIds is List<int> orderIdList)
                {
                    foreach (var orderId in orderIdList)
                    {
                        var order = Order.Orders.FirstOrDefault(o => o.OrderId == orderId);
                        if (order != null && order.Client != null)
                        {
                            RouteItems.Add(new RouteItemViewModel
                            {
                                ClientId = order.Client.ClientId,
                                ClientName = order.Client.ClientName ?? "Unknown",
                                Address = ParseAddress(order.Client.ShipToAddress),
                                Sequence = RouteItems.Count + 1,
                                OrderId = orderId
                            });
                        }
                    }
                    UpdateSequenceNumbers();
                }

                // Handle results from AddPOSToRoutePage
                if (query.TryGetValue("selectedClientIds", out var clientIds) && clientIds is List<int> clientIdList)
                {
                    foreach (var clientId in clientIdList)
                    {
                        var client = Client.Find(clientId);
                        if (client != null)
                        {
                            RouteItems.Add(new RouteItemViewModel
                            {
                                ClientId = client.ClientId,
                                ClientName = client.ClientName ?? "Unknown",
                                Address = ParseAddress(client.ShipToAddress),
                                Sequence = RouteItems.Count + 1
                            });
                        }
                    }
                    UpdateSequenceNumbers();
                }
            }
        }

        private string ParseAddress(string addressString)
        {
            if (string.IsNullOrEmpty(addressString))
                return "No address";

            var parts = addressString.Split('|');
            return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        private void UpdateSequenceNumbers()
        {
            for (int i = 0; i < RouteItems.Count; i++)
            {
                RouteItems[i].Sequence = i + 1;
            }
        }

        [RelayCommand]
        private void MoveItemUp(RouteItemViewModel item)
        {
            var index = RouteItems.IndexOf(item);
            if (index > 0)
            {
                RouteItems.Move(index, index - 1);
                UpdateSequenceNumbers();
            }
        }

        [RelayCommand]
        private void MoveItemDown(RouteItemViewModel item)
        {
            var index = RouteItems.IndexOf(item);
            if (index < RouteItems.Count - 1)
            {
                RouteItems.Move(index, index + 1);
                UpdateSequenceNumbers();
            }
        }

        [RelayCommand]
        private void DeleteItem(RouteItemViewModel item)
        {
            RouteItems.Remove(item);
            UpdateSequenceNumbers();
        }

        [RelayCommand]
        private async Task SelectDriver()
        {
            var trucks = Truck.Trucks.Where(x => x.DriverId > 0).ToList();
            
            if (trucks.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No trucks with drivers available.", "Info", "OK");
                return;
            }

            var truckNames = trucks.Select(x => x.Name ?? $"Truck {x.Id}").ToArray();
            var selectedTruckName = await _dialogService.ShowActionSheetAsync("Select Driver", "Cancel", null, truckNames);
            
            if (!string.IsNullOrEmpty(selectedTruckName) && selectedTruckName != "Cancel")
            {
                var selectedTruck = trucks.FirstOrDefault(x => x.Name == selectedTruckName);
                if (selectedTruck != null && selectedTruck.DriverId.HasValue)
                {
                    SelectedDriver = selectedTruck.Name ?? "Unknown";
                    SelectedDriverId = selectedTruck.DriverId.Value;
                    SelectedSalesman = "Not Selected";
                    SelectedSalesmanId = 0;
                    
                    // Clear current route when driver changes
                    _currentRoute = null;
                    RouteItems.Clear();
                }
            }
        }

        [RelayCommand]
        private async Task SelectSalesman()
        {
            SalesmanRole selectedRole = SalesmanRole.Presale | SalesmanRole.DSD;
            var salesmen = Salesman.List
                .Where(x => ((int)x.Roles & (int)selectedRole) > 0 && !x.Name.ToLowerInvariant().Contains("sf :"))
                .OrderBy(x => x.Name)
                .ToList();

            // Exclude salesmen who are drivers (have trucks)
            var salesmanWithTruck = new List<Salesman>();
            foreach (var truck in Truck.Trucks)
            {
                var salesman = Salesman.List.FirstOrDefault(x => x.Id == truck.DriverId);
                if (salesman != null && !salesmanWithTruck.Contains(salesman))
                    salesmanWithTruck.Add(salesman);
            }

            foreach (var s in salesmanWithTruck)
            {
                if (salesmen.Contains(s))
                    salesmen.Remove(s);
            }

            if (salesmen.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No salesmen available.", "Info", "OK");
                return;
            }

            var salesmanNames = salesmen.Select(x => x.Name).ToArray();
            var selectedSalesmanName = await _dialogService.ShowActionSheetAsync("Select Salesman", "Cancel", null, salesmanNames);
            
            if (!string.IsNullOrEmpty(selectedSalesmanName) && selectedSalesmanName != "Cancel")
            {
                var selected = salesmen.FirstOrDefault(x => x.Name == selectedSalesmanName);
                if (selected != null)
                {
                    SelectedSalesman = selected.Name;
                    SelectedSalesmanId = selected.Id;
                    SelectedDriver = "Not Selected";
                    SelectedDriverId = 0;
                    
                    // Clear current route when salesman changes
                    _currentRoute = null;
                    RouteItems.Clear();
                }
            }
        }

        [RelayCommand]
        private async Task SelectDate()
        {
            // Use date picker - for now use prompt, but ideally would use DatePicker
            var dateText = await _dialogService.ShowPromptAsync("Select Date", "Enter date (MM/DD/YYYY)", "OK", "Cancel", RouteDate.ToShortDateString(), -1, "");
            
            if (!string.IsNullOrEmpty(dateText) && DateTime.TryParse(dateText, out var selectedDate))
            {
                RouteDate = selectedDate;
                SelectedDate = RouteDate.ToShortDateString();
                
                // Reload route for new date
                await Refresh();
            }
        }

        [RelayCommand]
        private async Task ViewMap()
        {
            await Shell.Current.GoToAsync("routemap");
        }

        [RelayCommand]
        private async Task Refresh()
        {
            if (SelectedDriverId == 0 && SelectedSalesmanId == 0)
            {
                await _dialogService.ShowAlertAsync("Please select a driver or salesman first.", "Info", "OK");
                return;
            }

            try
            {
                IsLoading = true;
                
                int vendorId = SelectedSalesmanId > 0 ? SelectedSalesmanId : SelectedDriverId;
                _currentRoute = DataAccess.GetRouteForDriverShipDate(vendorId, RouteDate);
                
                if (_currentRoute != null)
                {
                    _currentRoute.Details = _currentRoute.Details.OrderBy(x => x.Stop).ToList();
                    _currentRoute.Save(_routeFileName);
                    
                    // Reload route items
                    RouteItems.Clear();
                    foreach (var detail in _currentRoute.Details.Where(x => !x.Deleted).OrderBy(x => x.Stop))
                    {
                        Client? client = null;
                        if (detail.ClientId > 0)
                        {
                            client = Client.Find(detail.ClientId);
                        }
                        else if (detail.Order != null)
                        {
                            client = Client.Find(detail.Order.ClientId);
                        }

                        if (client != null)
                        {
                            RouteItems.Add(new RouteItemViewModel
                            {
                                ClientId = client.ClientId,
                                ClientName = client.ClientName ?? "Unknown",
                                Address = ParseAddress(client.ShipToAddress),
                                Sequence = detail.Stop,
                                OrderId = detail.Order?.Id ?? 0
                            });
                        }
                    }
                    UpdateSequenceNumbers();
                }
                else
                {
                    // Create new empty route
                    _currentRoute = new DriverRoute
                    {
                        DriverId = vendorId,
                        DateTicks = RouteDate.Ticks,
                        Details = new List<DriverRouteDetails>()
                    };
                    RouteItems.Clear();
                }
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error refreshing route: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [ObservableProperty] private bool _isLoading;

        [RelayCommand]
        private async Task AddPosition()
        {
            await Shell.Current.GoToAsync("addpostoroute");
        }

        [RelayCommand]
        private async Task AddOrder()
        {
            await Shell.Current.GoToAsync("addorderstoroute");
        }

        [RelayCommand]
        private async Task AddQuotes()
        {
            // TODO: Implement add quotes
            await _dialogService.ShowAlertAsync("Add quotes to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task Save()
        {
            if (SelectedDriverId == 0 && SelectedSalesmanId == 0)
            {
                await _dialogService.ShowAlertAsync("Please select a driver or salesman.", "Validation Error", "OK");
                return;
            }

            if (_currentRoute == null)
            {
                await _dialogService.ShowAlertAsync("No route to save. Please refresh first.", "Validation Error", "OK");
                return;
            }

            if (_currentRoute.Locked)
            {
                await _dialogService.ShowAlertAsync("This route is locked. The salesman/driver assigned to it already downloaded the stops.", "Warning", "OK");
                return;
            }

            var confirmed = await _dialogService.ShowConfirmationAsync("Save Route", "Are you sure you want to save the changes?", "Yes", "No");
            if (!confirmed)
                return;

            try
            {
                IsLoading = true;

                // Update current route with route items
                _currentRoute.Details.Clear();
                
                foreach (var item in RouteItems)
                {
                    var client = Client.Find(item.ClientId);
                    Order? order = null;
                    if (item.OrderId > 0)
                    {
                        order = Order.Orders.FirstOrDefault(o => o.OrderId == item.OrderId);
                    }

                    var detail = new DriverRouteDetails
                    {
                        ClientId = item.ClientId,
                        ClientName = item.ClientName,
                        Stop = item.Sequence,
                        Order = order != null ? new DriverRouteOrder
                        {
                            Id = order.OrderId,
                            ClientId = order.Client.ClientId,
                            OrderNumber = order.PrintedOrderId
                        } : null,
                        Deleted = false
                    };
                    
                    _currentRoute.Details.Add(detail);
                }

                // Save route locally
                _currentRoute.Save(_routeFileName);

                // Check for order changes before saving
                var ordersToCheck = RouteItems
                    .Where(x => x.OrderId > 0)
                    .Select(x => Order.Orders.FirstOrDefault(o => o.OrderId == x.OrderId))
                    .Where(o => o != null)
                    .Select(o => new DriverRouteOrder
                    {
                        Id = o!.OrderId,
                        ClientId = o.Client.ClientId,
                        OrderNumber = o.PrintedOrderId
                    })
                    .ToList();

                if (ordersToCheck.Count > 0)
                {
                    string result = DataAccess.CheckOrderChangesBeforeSaveRoute(ordersToCheck);
                    if (!string.IsNullOrEmpty(result))
                    {
                        var parts = result.Split(',').ToList();
                        string message = parts.Count == 1
                            ? $"The transaction: {string.Join(" ", parts)} was changed by another user. Do you want to continue saving the route?"
                            : $"The transactions: {string.Join(" ", parts)} were changed by another user. Do you want to continue saving the route?";

                        var continueSave = await _dialogService.ShowConfirmationAsync("Warning", message, "Yes", "No");
                        if (!continueSave)
                        {
                            IsLoading = false;
                            return;
                        }
                    }
                }

                // Save route to server
                DataAccess.SaveRoute(_routeFileName);
                
                await _dialogService.ShowAlertAsync("Route saved successfully.", "Success", "OK");
                
                // Clean up temp file
                if (File.Exists(_routeFileName))
                    File.Delete(_routeFileName);
                
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving route: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    public partial class RouteItemViewModel : ObservableObject
    {
        [ObservableProperty] private int _clientId;
        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _address = string.Empty;
        [ObservableProperty] private int _sequence;
        [ObservableProperty] private int _orderId;
    }
}
