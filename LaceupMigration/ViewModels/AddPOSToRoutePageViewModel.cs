using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class AddPOSToRoutePageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<ClientRouteItemViewModel> _allClients = new();

        [ObservableProperty] private ObservableCollection<ClientRouteItemViewModel> _clients = new();
        [ObservableProperty] private string _searchText = string.Empty;

        public AddPOSToRoutePageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                // TODO: Load available clients, excluding those already in route
                _allClients = Client.AllClients
                    .Select(c => new ClientRouteItemViewModel
                    {
                        ClientId = c.ClientId,
                        ClientName = c.ClientName ?? "Unknown",
                        Address = ParseAddress(c.ShipToAddress),
                        IsSelected = false
                    })
                    .ToList();

                FilterClients(string.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading clients: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private string ParseAddress(string addressString)
        {
            if (string.IsNullOrEmpty(addressString))
                return "No address";

            var parts = addressString.Split('|');
            return string.Join(", ", parts.Where(p => !string.IsNullOrWhiteSpace(p)));
        }

        public void FilterClients(string searchText)
        {
            Clients.Clear();

            var filtered = string.IsNullOrWhiteSpace(searchText)
                ? _allClients
                : _allClients.Where(x => 
                    x.ClientName?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true ||
                    x.Address?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true).ToList();

            foreach (var client in filtered)
            {
                Clients.Add(client);
            }
        }

        [RelayCommand]
        private void SelectAll()
        {
            foreach (var client in Clients)
            {
                client.IsSelected = true;
            }
        }

        [RelayCommand]
        private async Task AddSelected()
        {
            var selectedClients = Clients.Where(x => x.IsSelected).ToList();
            if (selectedClients.Count == 0)
            {
                await _dialogService.ShowAlertAsync("Please select at least one client.", "Info", "OK");
                return;
            }

            try
            {
                // TODO: Add selected clients as positions to route
                var result = new Dictionary<string, object>
                {
                    { "selectedClientIds", selectedClients.Select(x => x.ClientId).ToList() }
                };

                await Shell.Current.GoToAsync("..", result);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error adding positions: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }
    }

    public partial class ClientRouteItemViewModel : ObservableObject
    {
        [ObservableProperty] private int _clientId;
        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _address = string.Empty;
        [ObservableProperty] private bool _isSelected;
    }
}

