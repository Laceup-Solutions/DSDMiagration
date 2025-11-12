using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class AddClientBillToPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private string _address1 = string.Empty;
        [ObservableProperty] private string _address2 = string.Empty;
        [ObservableProperty] private string _city = string.Empty;
        [ObservableProperty] private string _state = string.Empty;
        [ObservableProperty] private string _zip = string.Empty;
        [ObservableProperty] private string _country = string.Empty;

        public AddClientBillToPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public void OnNavigatedTo(IDictionary<string, object> query)
        {
            if (query != null && query.TryGetValue("currentAddress", out var currentAddress))
            {
                // Parse existing address if provided
                if (currentAddress is string addressString && !string.IsNullOrEmpty(addressString))
                {
                    var parts = addressString.Split('|');
                    if (parts.Length > 0) Address1 = parts[0];
                    if (parts.Length > 1) Address2 = parts[1];
                    if (parts.Length > 2) City = parts[2];
                    if (parts.Length > 3) State = parts[3];
                    if (parts.Length > 4) Zip = parts[4];
                }
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Address1) || string.IsNullOrWhiteSpace(City) || 
                string.IsNullOrWhiteSpace(State) || string.IsNullOrWhiteSpace(Zip))
            {
                await _dialogService.ShowAlertAsync("Address 1, City, State, and Zip are required.", "Validation Error", "OK");
                return;
            }

            try
            {
                var addressParts = new List<string> { Address1, Address2, City, State, Zip };
                if (!string.IsNullOrWhiteSpace(Country))
                    addressParts.Add(Country);
                
                var addressString = string.Join("|", addressParts);

                var result = new Dictionary<string, object>
                {
                    { "billToAddress", addressString }
                };

                await Shell.Current.GoToAsync("..", result);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving address: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task Cancel()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}

