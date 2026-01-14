using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class ProductDetailsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private bool _isInitialLoad = true;
        [ObservableProperty]
        private Product? _product;

        [ObservableProperty]
        private Client? _client;

        [ObservableProperty]
        private string _productName = string.Empty;

        [ObservableProperty]
        private string _productImagePath = string.Empty;

        [ObservableProperty]
        private bool _hasImage;

        [ObservableProperty]
        private string _upc = string.Empty;

        [ObservableProperty]
        private string _code = string.Empty;

        [ObservableProperty]
        private string _packaging = "1";

        [ObservableProperty]
        private string _regularPrice = string.Empty;

        private double _regularPriceValue = 0;
        private double _salesPriceValue = 0;
        private double _unitPriceValue = 0;

        [ObservableProperty]
        private string _salesPrice = string.Empty;

        [ObservableProperty]
        private string _unitPrice = string.Empty;

        [ObservableProperty]
        private string _lowestPrice = string.Empty;

        [ObservableProperty]
        private string _onHand = string.Empty;

        [ObservableProperty]
        private string _truckInventory = string.Empty;

        [ObservableProperty]
        private string _cost = string.Empty;

        [ObservableProperty]
        private string _palletCapacity = "0";

        [ObservableProperty]
        private string _retailPrice = string.Empty;

        [ObservableProperty]
        private string _retailPriceUnit = string.Empty;

        [ObservableProperty]
        private int _retailPricePercent = 0;

        [ObservableProperty]
        private bool _showPrice = true;

        [ObservableProperty]
        private bool _showCost = false;

        [ObservableProperty]
        private bool _showLowestPriceLevel = false;

        [ObservableProperty]
        private bool _showUomSpinner = false;

        [ObservableProperty]
        private ObservableCollection<string> _uomList = new();

        [ObservableProperty]
        private string _selectedUom = string.Empty;

        [ObservableProperty]
        private int _selectedUomIndex = -1;

        [ObservableProperty]
        private ObservableCollection<ExtraPropertyItem> _extraProperties = new();

        [ObservableProperty]
        private bool _hasExtraProperties = false;

        public ProductDetailsPageViewModel(DialogService dialogService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _advancedOptionsService = advancedOptionsService;
        }

        public void Initialize(int productId, int? clientId = null)
        {
            _product = Product.Products.FirstOrDefault(x => x.ProductId == productId);
            if (_product == null)
                return;

            if (clientId.HasValue)
            {
                _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId.Value);
            }

            ProductName = _product.Name;
            Upc = _product.Upc ?? string.Empty;
            Code = _product.Code ?? string.Empty;

            // Get packaging
            int package = 1;
            if (!string.IsNullOrEmpty(_product.Package))
                int.TryParse(_product.Package, out package);
            Packaging = package.ToString();

            // Get product image
            var imagePath = ProductImage.GetProductImage(_product.ProductId);
            ProductImagePath = imagePath ?? string.Empty;
            HasImage = !string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath);

            // Show cost based on config
            ShowCost = Config.ShowCostInTemplate || Config.StartingPercentageBasedOnCost > 0 || Config.OnlyShowCostInProductDetails;
            ShowLowestPriceLevel = Config.ShowLowestPriceLevel;
            ShowPrice = !Config.HidePriceInTransaction;

            // Handle UoM
            if (!string.IsNullOrEmpty(_product.UoMFamily))
            {
                var familyUoms = UnitOfMeasure.List.Where(x => x.FamilyId == _product.UoMFamily).ToList();
                if (familyUoms != null && familyUoms.Count > 0)
                {
                    ShowUomSpinner = true;
                    UomList.Clear();
                    foreach (var uom in familyUoms)
                    {
                        UomList.Add(uom.Name);
                    }

                    // Set default UoM
                    var defaultUom = familyUoms.FirstOrDefault(x => x.IsDefault);
                    if (_client != null && _client.UseBaseUoM)
                        defaultUom = familyUoms.FirstOrDefault(x => x.IsBase);

                    if (defaultUom != null)
                    {
                        SelectedUom = defaultUom.Name;
                        SelectedUomIndex = familyUoms.IndexOf(defaultUom);
                    }
                }
            }

            CalculatePrices();
            LoadExtraProperties();
            _isInitialLoad = false; // Mark initial load as complete
        }

        private void CalculatePrices()
        {
            if (_product == null)
                return;

            int package = 1;
            double conversion = 1;
            double weight = 1;

            if (Config.IncludeAvgWeightInCatalogPrice && _product.Weight > 0)
                weight = _product.Weight;

            int.TryParse(_product.Package, out package);

            UnitOfMeasure uom = null;
            if (!string.IsNullOrEmpty(SelectedUom) && !string.IsNullOrEmpty(_product.UoMFamily))
            {
                uom = UnitOfMeasure.List.FirstOrDefault(x => x.Name == SelectedUom && x.FamilyId == _product.UoMFamily);
                if (uom != null)
                    conversion = uom.Conversion;
            }

            conversion *= weight;

            // Calculate prices
            double rprice = 0;
            double sprice = 0;
            double unitPrice = 0;

            if (_client != null)
            {
                rprice = Product.GetPriceForProduct(_product, _client, false, false);
                sprice = Product.GetPriceForProduct(_product, _client, true, false);
            }
            else
            {
                rprice = _product.PriceLevel0;
                sprice = _product.PriceLevel0;
            }

            rprice = _product.PriceLevel0 * conversion;
            sprice = sprice * conversion;

            unitPrice = sprice / package;
            if (package == 1)
                unitPrice /= conversion;

            _regularPriceValue = rprice;
            _salesPriceValue = sprice;
            _unitPriceValue = unitPrice;
            RegularPrice = rprice.ToCustomString();
            SalesPrice = sprice.ToCustomString();
            UnitPrice = unitPrice.ToCustomString();
            LowestPrice = (_product.LowestAcceptablePrice * conversion).ToCustomString();

            // Inventory - matches Xamarin behavior:
            // Initial load (line 320): Truck Inventory = CurrentInventory (NO conversion)
            // RecalculatePrices (line 868): Truck Inventory = CurrentInventory / uom.Conversion (WITH conversion)
            if (uom != null)
            {
                OnHand = Math.Round((_product.CurrentWarehouseInventory / uom.Conversion), Config.Round).ToString();
                
                // Apply conversion only if NOT initial load (matches Xamarin RecalculatePrices line 868)
                if (!_isInitialLoad)
                {
                    TruckInventory = Math.Round((_product.CurrentInventory / uom.Conversion), Config.Round).ToString();
                }
                else
                {
                    // Initial load: no conversion (matches Xamarin line 320)
                    // Xamarin: truckInventory.Text = product.CurrentInventory.ToString();
                    // Product.CurrentInventory already applies rounding based on Config.Round
                    TruckInventory = _product.CurrentInventory.ToString();
                }
            }
            else
            {
                OnHand = _product.CurrentWarehouseInventory.ToString();
                TruckInventory = _product.CurrentInventory.ToString();
            }

            // Cost
            if (ShowCost)
            {
                Cost = (_product.Cost * conversion).ToCustomString();
            }

            // Retail price (initial calculation) - starts with sales price (matches Xamarin: gRegPrice = sprice)
            RetailPrice = sprice.ToCustomString();
            RetailPriceUnit = $"Retail Price(Unit): {unitPrice.ToCustomString()}";
            RetailPricePercent = 0;

            // Pallet Capacity (matches Xamarin: ProductDetailsPalletCapacity.Text = product.PalletSize.ToString())
            PalletCapacity = _product.PalletSize.ToString();
        }

        private void LoadExtraProperties()
        {
            ExtraProperties.Clear();
            HasExtraProperties = false;
            
            if (_product == null)
                return;
                
            // Check if ExtraPropertiesAsString exists and is not empty (matches Xamarin logic)
            if (string.IsNullOrEmpty(_product.ExtraPropertiesAsString))
                return;
                
            // Access ExtraProperties to ensure it's loaded
            var extraProps = _product.ExtraProperties;
            
            if (extraProps != null && extraProps.Count > 0)
            {
                foreach (var tuple in extraProps)
                {
                    // Skip certain system properties that shouldn't be displayed
                    if (string.IsNullOrEmpty(tuple.Item1) || string.IsNullOrEmpty(tuple.Item2))
                        continue;

                    ExtraProperties.Add(new ExtraPropertyItem
                    {
                        Key = tuple.Item1,
                        Value = tuple.Item2
                    });
                }
                
                // Only show if we have properties to display
                HasExtraProperties = ExtraProperties.Count > 0;
            }
        }

        partial void OnSelectedUomIndexChanged(int value)
        {
            if (value >= 0 && value < UomList.Count)
            {
                SelectedUom = UomList[value];
                CalculatePrices();
            }
        }

        partial void OnRetailPricePercentChanged(int value)
        {
            if (_product == null)
                return;

            // Use the stored sales price value (matches Xamarin: gRegPrice = sprice, line 295)
            // This ensures we're calculating from the exact same base price used in Xamarin
            double basePrice = _salesPriceValue;
            double baseUnitPrice = _unitPriceValue;
            
            if (basePrice == 0)
            {
                // Fallback: recalculate if not set yet
                int package = 1;
                double conversion = 1;
                double weight = 1;

                if (Config.IncludeAvgWeightInCatalogPrice && _product.Weight > 0)
                    weight = _product.Weight;

                int.TryParse(_product.Package, out package);

                UnitOfMeasure uom = null;
                if (!string.IsNullOrEmpty(SelectedUom) && !string.IsNullOrEmpty(_product.UoMFamily))
                {
                    uom = UnitOfMeasure.List.FirstOrDefault(x => x.Name == SelectedUom && x.FamilyId == _product.UoMFamily);
                    if (uom != null)
                        conversion = uom.Conversion;
                }

                conversion *= weight;
                
                if (_client != null)
                {
                    basePrice = Product.GetPriceForProduct(_product, _client, true, false);
                }
                else
                {
                    basePrice = _product.PriceLevel0;
                }
                basePrice = basePrice * conversion;
                
                baseUnitPrice = basePrice / package;
                if (package == 1)
                    baseUnitPrice /= conversion;
            }

            // Calculate retail price using exact Xamarin formula: gRegPrice / percentage
            // Where percentage = 1 - (e.Progress * 0.01)
            // This matches Xamarin SeekBar_ProgressChanged (line 890-897)
            double percentage = 1.0 - (value * 0.01);
            double retailPrice = basePrice / percentage;
            double retailUnitPrice = baseUnitPrice / percentage;

            RetailPrice = retailPrice.ToCustomString();
            RetailPriceUnit = $"Retail Price(Unit): {retailUnitPrice.ToCustomString()}";
        }

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            var options = new List<string> { "Print Label", "Advanced Options" };
            
            var choice = await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null, options.ToArray());
            
            if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
                return;
            
            switch (choice)
            {
                case "Print Label":
                    await PrintProductLabelAsync();
                    break;
                case "Advanced Options":
                    await ShowAdvancedOptionsAsync();
                    break;
            }
        }

        private async Task ShowAdvancedOptionsAsync()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
        }

        public async Task PrintProductLabelAsync()
        {
            if (_product == null)
                return;

            try
            {
                PrinterProvider.PrintDocument((int number) =>
                {
                    if (number < 1)
                        return "Please enter a valid number of copies.";

                    // Generate label string (matches Xamarin GenerateLabel method)
                    string labelDefinition = @"
            ^XA^PR4^MD17
            ^FO20,10^ADN,18,10^FD{PRODUCTNAME}^FS
            ^FO50,40^BY3^BUN,58^FD{BARCODE}^FS
            ^FO150,145^ADN,18,10^FD{PRICE}^FS
            ^XZ";

                    double price = 0;
                    if (_client != null)
                        price = Product.GetPriceForProduct(_product, _client, false, false);
                    else
                        price = _product.PriceLevel0;

                    StringBuilder sb = new StringBuilder();
                    for (int x = 0; x < number; x++)
                    {
                        var labelAsString = labelDefinition.Replace("{PRODUCTNAME}", _product.Name);
                        labelAsString = labelAsString.Replace("{BARCODE}", _product.Upc ?? "");
                        labelAsString = labelAsString.Replace("{PRICE}", Math.Round(price, Config.Round).ToCustomString());
                        sb.Append(labelAsString);
                    }

                    CompanyInfo.SelectedCompany = CompanyInfo.Companies.Count > 0 ? CompanyInfo.Companies[0] : null;
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    
                    if (!printer.PrintProductLabel(sb.ToString()))
                        return "Error printing product label.";
                    
                    return string.Empty;
                });
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing product label: {ex.Message}", "Error", "OK");
                Logger.CreateLog(ex);
            }
        }
    }

    public partial class ExtraPropertyItem : ObservableObject
    {
        [ObservableProperty]
        private string _key = string.Empty;
        
        [ObservableProperty]
        private string _value = string.Empty;
    }
}
