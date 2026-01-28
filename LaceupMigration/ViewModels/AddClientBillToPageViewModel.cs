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
        [ObservableProperty] private bool _useSameAsShipTo;
        [ObservableProperty] private bool _isAddressEnabled = true;

        private string _shipToAddress1 = string.Empty;
        private string _shipToAddress2 = string.Empty;
        private string _shipToCity = string.Empty;
        private string _shipToState = string.Empty;
        private string _shipToZip = string.Empty;

        public AddClientBillToPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public void OnNavigatedTo(IDictionary<string, object> query)
        {
            // Store ship-to address
            if (query != null && query.TryGetValue("currentAddress", out var currentAddress))
            {
                // Parse existing address if provided (this is the ship to address)
                if (currentAddress is string addressString && !string.IsNullOrEmpty(addressString))
                {
                    var parts = addressString.Split('|');
                    _shipToAddress1 = parts.Length > 0 ? parts[0] : string.Empty;
                    _shipToAddress2 = parts.Length > 1 ? parts[1] : string.Empty;
                    _shipToCity = parts.Length > 2 ? parts[2] : string.Empty;
                    _shipToState = parts.Length > 3 ? parts[3] : string.Empty;
                    _shipToZip = parts.Length > 4 ? parts[4] : string.Empty;
                }
            }
            
            // Pre-populate with existing Bill To address if provided (for editing)
            if (query != null && query.TryGetValue("billToAddress", out var billToAddress))
            {
                if (billToAddress is string billToAddressString && !string.IsNullOrEmpty(billToAddressString))
                {
                    var parts = billToAddressString.Split('|');
                    Address1 = parts.Length > 0 ? parts[0] : string.Empty;
                    Address2 = parts.Length > 1 ? parts[1] : string.Empty;
                    City = parts.Length > 2 ? parts[2] : string.Empty;
                    State = parts.Length > 3 ? parts[3] : string.Empty;
                    Zip = parts.Length > 4 ? parts[4] : string.Empty;
                    Country = parts.Length > 5 ? parts[5] : string.Empty;
                    
                    // Check if Bill To matches Ship To
                    var shipToAddress = $"{_shipToAddress1}|{_shipToAddress2}|{_shipToCity}|{_shipToState}|{_shipToZip}";
                    UseSameAsShipTo = (billToAddressString == shipToAddress);
                }
            }
            else
            {
                // Fields start empty if no existing Bill To address
                Address1 = string.Empty;
                Address2 = string.Empty;
                City = string.Empty;
                State = string.Empty;
                Zip = string.Empty;
            }
        }

        partial void OnUseSameAsShipToChanged(bool value)
        {
            if (value)
            {
                // Copy ship to address to bill to address
                Address1 = _shipToAddress1;
                Address2 = _shipToAddress2;
                City = _shipToCity;
                State = _shipToState;
                Zip = _shipToZip;
                IsAddressEnabled = false;
            }
            else
            {
                // Clear fields when unchecked
                Address1 = string.Empty;
                Address2 = string.Empty;
                City = string.Empty;
                State = string.Empty;
                Zip = string.Empty;
                IsAddressEnabled = true;
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            // If "Use same as Ship To Address" is checked, skip validation (match Xamarin behavior)
            if (!UseSameAsShipTo)
            {
                if (string.IsNullOrWhiteSpace(Address1) || string.IsNullOrWhiteSpace(City) || 
                    string.IsNullOrWhiteSpace(State) || string.IsNullOrWhiteSpace(Zip))
                {
                    await _dialogService.ShowAlertAsync("Address 1, City, State, and Zip are required.", "Validation Error", "OK");
                    return;
                }
            }

            try
            {
                // If checkbox is checked, use ship-to address values
                string finalAddress1, finalAddress2, finalCity, finalState, finalZip;
                
                if (UseSameAsShipTo)
                {
                    finalAddress1 = _shipToAddress1;
                    finalAddress2 = _shipToAddress2;
                    finalCity = _shipToCity;
                    finalState = _shipToState;
                    finalZip = _shipToZip;
                }
                else
                {
                    finalAddress1 = Address1;
                    finalAddress2 = Address2;
                    finalCity = City;
                    finalState = State;
                    finalZip = Zip;
                }

                var addressParts = new List<string> { finalAddress1, finalAddress2, finalCity, finalState, finalZip };
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

