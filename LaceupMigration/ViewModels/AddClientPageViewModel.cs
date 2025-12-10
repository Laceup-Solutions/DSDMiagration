using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class AddClientPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private string _name = string.Empty;
        [ObservableProperty] private string _contact = string.Empty;
        [ObservableProperty] private string _phone = string.Empty;
        [ObservableProperty] private string _email = string.Empty;
        [ObservableProperty] private string _address1 = string.Empty;
        [ObservableProperty] private string _address2 = string.Empty;
        [ObservableProperty] private string _city = string.Empty;
        [ObservableProperty] private string _state = string.Empty;
        [ObservableProperty] private string _zip = string.Empty;
        [ObservableProperty] private string _license = string.Empty;
        [ObservableProperty] private bool _canChangePrices;
        [ObservableProperty] private string _comment = string.Empty;
        [ObservableProperty] private bool _taxable;
        [ObservableProperty] private bool _oneDoc;
        [ObservableProperty] private string _taxRate = "0";
        [ObservableProperty] private string _selectedPriceLevel = "Not Selected";
        [ObservableProperty] private string _selectedTerms = "Not Selected";
        [ObservableProperty] private int _selectedPriceLevelId;
        [ObservableProperty] private int _selectedTermId;
        [ObservableProperty] private int _selectedRetailPriceLevelId;
        [ObservableProperty] private string _selectedRetailPriceLevel = "Not Selected";
        [ObservableProperty] private bool _showCanChangePrices;
        [ObservableProperty] private bool _showTerms;
        [ObservableProperty] private List<PriceLevel> _priceLevels = new();
        [ObservableProperty] private PriceLevel _selectedPriceLevelItem;
        [ObservableProperty] private List<RetailPriceLevel> _retailPriceLevels = new();
        [ObservableProperty] private RetailPriceLevel _selectedRetailPriceLevelItem;

        public AddClientPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            
            // Initialize visibility properties based on Config (matching EditClientPageViewModel)
            ShowCanChangePrices = Config.NewClientCanChangePrices;
            ShowTerms = Term.List.Count > 0 && Config.CanSelectTermsOnCreateClient;
            
            // Initialize price levels list
            PriceLevels = PriceLevel.List.OrderBy(x => x.Name).ToList();
            
            // Initialize retail price levels list
            RetailPriceLevels = RetailPriceLevel.Pricelist.OrderBy(x => x.Name).ToList();
        }

        partial void OnSelectedPriceLevelItemChanged(PriceLevel value)
        {
            if (value != null)
            {
                SelectedPriceLevelId = value.Id;
                SelectedPriceLevel = value.Name;
            }
            else
            {
                SelectedPriceLevelId = 0;
                SelectedPriceLevel = "Not Selected";
            }
        }

        partial void OnSelectedRetailPriceLevelItemChanged(RetailPriceLevel value)
        {
            if (value != null)
            {
                SelectedRetailPriceLevelId = value.Id;
                SelectedRetailPriceLevel = value.Name;
            }
            else
            {
                SelectedRetailPriceLevelId = 0;
                SelectedRetailPriceLevel = "Not Selected";
            }
        }

        public void OnNavigatedTo(IDictionary<string, object> query)
        {
            if (query != null)
            {
                if (query.TryGetValue("priceLevelId", out var priceLevelId) && priceLevelId != null)
                {
                    SelectedPriceLevelId = Convert.ToInt32(priceLevelId);
                    if (query.TryGetValue("priceLevelName", out var priceLevelName))
                    {
                        SelectedPriceLevel = priceLevelName?.ToString() ?? "Not Selected";
                    }
                    // Set the selected item for the picker
                    SelectedPriceLevelItem = PriceLevels.FirstOrDefault(x => x.Id == SelectedPriceLevelId);
                }

                if (query.TryGetValue("termId", out var termId) && termId != null)
                {
                    SelectedTermId = Convert.ToInt32(termId);
                    if (query.TryGetValue("termName", out var termName))
                    {
                        SelectedTerms = termName?.ToString() ?? "Not Selected";
                    }
                }

                if (query.TryGetValue("retailPriceLevelId", out var retailPriceLevelId) && retailPriceLevelId != null)
                {
                    SelectedRetailPriceLevelId = Convert.ToInt32(retailPriceLevelId);
                    if (query.TryGetValue("retailPriceLevelName", out var retailPriceLevelName))
                    {
                        SelectedRetailPriceLevel = retailPriceLevelName?.ToString() ?? "Not Selected";
                    }
                    // Set the selected item for the picker
                    SelectedRetailPriceLevelItem = RetailPriceLevels.FirstOrDefault(x => x.Id == SelectedRetailPriceLevelId);
                }
            }
        }

        [RelayCommand]
        private async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                await _dialogService.ShowAlertAsync("Client name is required.", "Validation Error", "OK");
                return;
            }

            if (Config.ClientNameMaxSize > 0 && Name.Length > Config.ClientNameMaxSize)
            {
                await _dialogService.ShowAlertAsync($"Client name must be {Config.ClientNameMaxSize} characters or less.", "Validation Error", "OK");
                return;
            }

            if (!string.IsNullOrEmpty(Email) && !IsValidEmail(Email))
            {
                await _dialogService.ShowAlertAsync("Invalid email address format.", "Validation Error", "OK");
                return;
            }

            // Check for duplicate client name
            string nameLower = Name.ToLowerInvariant().Trim();
            if (Client.Clients.Any(x => x.ClientName.ToLowerInvariant().Trim() == nameLower))
            {
                await _dialogService.ShowAlertAsync("A client with this name already exists.", "Validation Error", "OK");
                return;
            }

            try
            {
                // Get next negative ID for new client
                int nextId = Client.NextAddedId() - 1;

                var addressParts = new List<string> { Address1, Address2, City, State, Zip };
                var addressString = string.Join("|", addressParts);

                var client = new Client
                {
                    ClientId = nextId,
                    ClientName = Name,
                    ContactName = Contact,
                    ContactPhone = Phone,
                    BillToAddress = addressString,
                    ShipToAddress = addressString,
                    LicenceNumber = License,
                    Comment = Comment ?? string.Empty,
                    Taxable = Taxable,
                    OneDoc = OneDoc,
                    PriceLevel = SelectedPriceLevelId,
                    TermId = SelectedTermId,
                    RetailPriceLevelId = SelectedRetailPriceLevelId,
                    Editable = true,
                    CategoryId = 0, // Default category
                    OriginalId = string.Empty,
                    UniqueId = Guid.NewGuid().ToString("N"),
                    CommId = string.Empty,
                    DUNS = string.Empty,
                    Location = string.Empty,
                    OpenBalance = 0,
                    OverCreditLimit = false
                };

                if (double.TryParse(TaxRate, out var taxRateValue))
                {
                    client.TaxRate = taxRateValue;
                }

                // Handle email in extra properties
                if (!string.IsNullOrEmpty(Email))
                {
                    client.ExtraPropertiesAsString = DataAccess.SyncSingleUDF("email", Email, client.ExtraPropertiesAsString);
                }

                // Handle can change prices
                if (CanChangePrices)
                {
                    client.NonvisibleExtraPropertiesAsString = DataAccess.SyncSingleUDF("pricechangeable", "yes", client.NonvisibleExtraPropertiesAsString);
                }

                // Handle use LSP
                client.NonvisibleExtraPropertiesAsString = DataAccess.SyncSingleUDF("uselsp", "yes", client.NonvisibleExtraPropertiesAsString);

                // Handle license number
                if (!string.IsNullOrEmpty(License))
                {
                    client.NonvisibleExtraPropertiesAsString = DataAccess.SyncSingleUDF("EIN #", License, client.NonvisibleExtraPropertiesAsString);
                }

                // Handle new client discount if configured
                if (Config.NewClientCanHaveDiscount)
                {
                    client.ExtraPropertiesAsString = DataAccess.SyncSingleUDF("allowDiscount", "1", client.ExtraPropertiesAsString);
                }

                // Handle CoolerCo customization
                if (Config.CoolerCoCustomization && !State.ToLower().Contains("ca"))
                {
                    client.ExtraPropertiesAsString = DataAccess.SyncSingleUDF("userelated", "no", client.ExtraPropertiesAsString);
                }

                // Set default OneDoc based on customization
                if (Config.CoolerCoCustomization)
                {
                    client.OneDoc = false;
                }
                else
                {
                    client.OneDoc = OneDoc;
                }

                // Set default tax checkbox
                if (Config.DefaultTaxRate > 0)
                {
                    client.Taxable = true;
                    if (double.TryParse(TaxRate, out var defaultTaxRate))
                    {
                        client.TaxRate = defaultTaxRate;
                    }
                    else
                    {
                        client.TaxRate = Config.DefaultTaxRate;
                    }
                }

                // Add client to the list
                Client.AddClient(client);
                
                // Save all clients
                Client.Save();

                await _dialogService.ShowAlertAsync("Client added successfully.", "Success", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error adding client: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        [RelayCommand]
        private async Task SelectPriceLevel()
        {
            var levels = PriceLevel.List.OrderBy(x => x.Name).ToList();
            var levelNames = levels.Select(x => x.Name).ToArray();
            var currentIndex = SelectedPriceLevelId > 0 ? levels.FindIndex(x => x.Id == SelectedPriceLevelId) : -1;

            var selectedIndex = await _dialogService.ShowSelectionAsync("Select Price Level", levelNames);
            if (selectedIndex >= 0 && selectedIndex < levels.Count)
            {
                SelectedPriceLevelId = levels[selectedIndex].Id;
                SelectedPriceLevel = levels[selectedIndex].Name;
            }
        }

        [RelayCommand]
        private async Task SelectTerms()
        {
            var terms = Term.List.OrderBy(x => x.Name).ToList();
            var termNames = terms.Select(x => x.Name).ToArray();
            var currentIndex = SelectedTermId > 0 ? terms.FindIndex(x => x.Id == SelectedTermId) : -1;

            var selectedIndex = await _dialogService.ShowSelectionAsync("Select Terms", termNames);
            if (selectedIndex >= 0 && selectedIndex < terms.Count)
            {
                SelectedTermId = terms[selectedIndex].Id;
                SelectedTerms = terms[selectedIndex].Name;
            }
        }

        [RelayCommand]
        private async Task SelectRetailPriceLevel()
        {
            var levels = RetailPriceLevel.Pricelist.OrderBy(x => x.Name).ToList();
            var levelNames = levels.Select(x => x.Name).ToArray();
            var currentIndex = SelectedRetailPriceLevelId > 0 ? levels.FindIndex(x => x.Id == SelectedRetailPriceLevelId) : -1;

            var selectedIndex = await _dialogService.ShowSelectionAsync("Select Retail Price Level", levelNames);
            if (selectedIndex >= 0 && selectedIndex < levels.Count)
            {
                SelectedRetailPriceLevelId = levels[selectedIndex].Id;
                SelectedRetailPriceLevel = levels[selectedIndex].Name;
            }
        }

        [RelayCommand]
        private async Task SelectBillTo()
        {
            var currentAddress = $"{Address1}|{Address2}|{City}|{State}|{Zip}";
            var query = new Dictionary<string, object> { { "currentAddress", currentAddress } };
            await Shell.Current.GoToAsync("addclientbillto", query);
        }

        public void OnBillToSelected(IDictionary<string, object> query)
        {
            if (query != null && query.TryGetValue("billToAddress", out var billToAddress))
            {
                if (billToAddress is string addressString && !string.IsNullOrEmpty(addressString))
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
    }
}
