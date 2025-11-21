using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Linq;
using System.Collections.ObjectModel;

namespace LaceupMigration.ViewModels
{
    public partial class ProductDetailsPageViewModel : ObservableObject
    {
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

            RegularPrice = rprice.ToCustomString();
            SalesPrice = sprice.ToCustomString();
            UnitPrice = unitPrice.ToCustomString();
            LowestPrice = (_product.LowestAcceptablePrice * conversion).ToCustomString();

            // Inventory
            if (uom != null)
            {
                OnHand = Math.Round((_product.CurrentWarehouseInventory / uom.Conversion), 2).ToString();
            }
            else
            {
                OnHand = _product.CurrentWarehouseInventory.ToString();
            }

            TruckInventory = _product.CurrentInventory.ToString();

            // Cost
            if (ShowCost)
            {
                Cost = (_product.Cost * conversion).ToCustomString();
            }

            // Retail price (initial calculation)
            RetailPrice = sprice.ToCustomString();
            RetailPriceUnit = $"Retail Price(Unit): {unitPrice.ToCustomString()}";
            RetailPricePercent = 0;

            // Pallet Capacity
            PalletCapacity = "0"; // This would need to come from product data if available
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

            // Calculate retail price based on percentage
            double basePrice = 0;
            if (_client != null)
            {
                basePrice = Product.GetPriceForProduct(_product, _client, true, false);
            }
            else
            {
                basePrice = _product.PriceLevel0;
            }

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
            basePrice = basePrice * conversion;
            double unitPrice = basePrice / package;
            if (package == 1)
                unitPrice /= conversion;

            double retailPrice = basePrice * (1 + (value / 100.0));
            double retailUnitPrice = unitPrice * (1 + (value / 100.0));

            RetailPrice = retailPrice.ToCustomString();
            RetailPriceUnit = $"Retail Price(Unit): {retailUnitPrice.ToCustomString()}";
        }
    }
}
