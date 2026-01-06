using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class EditClientPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private Client? _client;
        private bool _initialized;

        [ObservableProperty]
        private string _name = string.Empty;

        [ObservableProperty]
        private string _contactName = string.Empty;

        [ObservableProperty]
        private string _phone = string.Empty;

        [ObservableProperty]
        private string _email = string.Empty;

        [ObservableProperty]
        private string _address1 = string.Empty;

        [ObservableProperty]
        private string _address2 = string.Empty;

        [ObservableProperty]
        private string _city = string.Empty;

        [ObservableProperty]
        private string _state = string.Empty;

        [ObservableProperty]
        private string _zip = string.Empty;

        [ObservableProperty]
        private string _priceLevelText = "Select Price Level";

        [ObservableProperty]
        private string _termsText = "Select Terms";

        [ObservableProperty]
        private bool _showTerms;

        [ObservableProperty]
        private bool _canChangePrices;

        [ObservableProperty]
        private bool _showCanChangePrices = true;

        [ObservableProperty]
        private bool _taxable;

        [ObservableProperty]
        private string _taxRate = "0";

        [ObservableProperty]
        private string _licenseNumber = string.Empty;

        [ObservableProperty]
        private bool _oneDoc;

        [ObservableProperty]
        private List<PriceLevel> _priceLevels = new();
        [ObservableProperty]
        private PriceLevel _selectedPriceLevelItem;
        [ObservableProperty]
        private List<RetailPriceLevel> _retailPriceLevels = new();
        [ObservableProperty]
        private RetailPriceLevel _selectedRetailPriceLevelItem;

        private PriceLevel? _selectedPriceLevel;
        private int _termId;
        private int _retailPriceLevelId;

        public EditClientPageViewModel(DialogService dialogService)
        {
            _dialogService = dialogService;
            ShowTerms = Term.List.Count > 0 && Config.CanSelectTermsOnCreateClient;
            ShowCanChangePrices = Config.NewClientCanChangePrices;
            
            // Initialize price levels lists
            PriceLevels = PriceLevel.List.OrderBy(x => x.Name).ToList();
            RetailPriceLevels = RetailPriceLevel.Pricelist.OrderBy(x => x.Name).ToList();
        }

        partial void OnSelectedPriceLevelItemChanged(PriceLevel value)
        {
            if (value != null)
            {
                _selectedPriceLevel = value;
                PriceLevelText = value.Name;
            }
            else
            {
                _selectedPriceLevel = null;
                PriceLevelText = "Select Price Level";
            }
        }

        partial void OnSelectedRetailPriceLevelItemChanged(RetailPriceLevel value)
        {
            if (value != null)
            {
                _retailPriceLevelId = value.Id;
            }
            else
            {
                _retailPriceLevelId = 0;
            }
        }

        public async Task InitializeAsync(int clientId)
        {
            if (_initialized && _client?.ClientId == clientId)
                return;

            _client = Client.Find(clientId);
            if (_client == null)
            {
                await _dialogService.ShowAlertAsync("Client not found.", "Error");
                return;
            }

            Name = _client.ClientName;
            ContactName = _client.ContactName;
            Phone = _client.ContactPhone;

            var email = UDFHelper.GetSingleUDF("email", _client.ExtraPropertiesAsString);
            Email = email ?? string.Empty;

            var addr = _client.ShipToAddress.Split('|');
            Address1 = addr.Length > 0 ? addr[0] : string.Empty;
            Address2 = addr.Length > 1 ? addr[1] : string.Empty;
            City = addr.Length > 2 ? addr[2] : string.Empty;
            State = addr.Length > 3 ? addr[3] : string.Empty;
            Zip = addr.Length > 4 ? addr[4] : string.Empty;

            var savedPriceLevel = PriceLevel.List.FirstOrDefault(x => x.Id == _client.PriceLevel);
            if (savedPriceLevel != null)
            {
                _selectedPriceLevel = savedPriceLevel;
                PriceLevelText = savedPriceLevel.Name;
                SelectedPriceLevelItem = savedPriceLevel;
            }

            var rprice = RetailPriceLevel.Pricelist.FirstOrDefault(x => x.Id == _client.RetailPriceLevelId);
            if (rprice != null)
            {
                _retailPriceLevelId = rprice.Id;
                SelectedRetailPriceLevelItem = rprice;
            }

            var savedTerm = Term.List.FirstOrDefault(x => x.Id == _client.TermId);
            if (savedTerm != null)
            {
                _termId = savedTerm.Id;
                TermsText = savedTerm.Name;
            }

            LicenseNumber = _client.LicenceNumber;
            Taxable = _client.Taxable;
            TaxRate = _client.TaxRate.ToString();
            OneDoc = _client.OneDoc;

            var hasField = UDFHelper.GetSingleUDF("pricechangeable", _client.NonvisibleExtraPropertiesAsString);
            CanChangePrices = hasField?.ToLowerInvariant() == "yes";

            _initialized = true;
        }

        public async Task OnAppearingAsync()
        {
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task SelectPriceLevelAsync()
        {
            var levels = PriceLevel.List.OrderBy(x => x.Name).ToList();
            var levelNames = levels.Select(x => x.Name).ToArray();
            var currentIndex = _selectedPriceLevel != null ? levels.IndexOf(_selectedPriceLevel) : -1;

            var selectedIndex = await _dialogService.ShowSelectionAsync("Select Price Level", levelNames);
            if (selectedIndex >= 0 && selectedIndex < levels.Count)
            {
                _selectedPriceLevel = levels[selectedIndex];
                PriceLevelText = _selectedPriceLevel.Name;
            }
        }

        [RelayCommand]
        private async Task SelectTermsAsync()
        {
            var terms = Term.List.OrderBy(x => x.Name).ToList();
            var termNames = terms.Select(x => x.Name).ToArray();
            var currentIndex = _termId > 0 ? terms.FindIndex(x => x.Id == _termId) : -1;

            var selectedIndex = await _dialogService.ShowSelectionAsync("Select Terms", termNames);
            if (selectedIndex >= 0 && selectedIndex < terms.Count)
            {
                _termId = terms[selectedIndex].Id;
                TermsText = terms[selectedIndex].Name;
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (_client == null)
                return;

            if (string.IsNullOrWhiteSpace(Name))
            {
                await _dialogService.ShowAlertAsync("Customer name is required.", "Error");
                return;
            }

            _client.ClientName = Name;
            _client.ContactName = ContactName;
            _client.ContactPhone = Phone;

            _client.ExtraPropertiesAsString = UDFHelper.SyncSingleUDF("email", Email, _client.ExtraPropertiesAsString);

            var address = $"{Address1}|{Address2}|{City}|{State}|{Zip}";
            _client.ShipToAddress = address;

            if (_selectedPriceLevel != null)
            {
                _client.PriceLevel = _selectedPriceLevel.Id;
            }

            if (_termId > 0)
            {
                _client.TermId = _termId;
            }

            if (_retailPriceLevelId > 0)
            {
                _client.RetailPriceLevelId = _retailPriceLevelId;
            }

            _client.LicenceNumber = LicenseNumber;
            _client.Taxable = Taxable;
            _client.OneDoc = OneDoc;
            if (double.TryParse(TaxRate, out var taxRateValue))
            {
                _client.TaxRate = taxRateValue;
            }

            _client.NonvisibleExtraPropertiesAsString = UDFHelper.SyncSingleUDF("pricechangeable", 
                CanChangePrices ? "yes" : "no", _client.NonvisibleExtraPropertiesAsString);

            Client.Save();

            await _dialogService.ShowAlertAsync("Client updated successfully.", "Success");
            await Shell.Current.GoToAsync("..");
        }
    }
}

