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
    public partial class SelectPriceLevelPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<PriceLevel> _allPriceLevels = new();
        private int? _orderId;
        private int? _productId;
        private int? _itemType;

        [ObservableProperty] private ObservableCollection<PriceLevelViewModel> _priceLevels = new();
        [ObservableProperty] private string _searchText = string.Empty;

        public SelectPriceLevelPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("orderId", out var orderValue) && orderValue != null)
            {
                if (int.TryParse(orderValue.ToString(), out var oId))
                    _orderId = oId;
            }

            if (query.TryGetValue("productId", out var productValue) && productValue != null)
            {
                if (int.TryParse(productValue.ToString(), out var pId))
                    _productId = pId;
            }

            if (query.TryGetValue("itemType", out var itemTypeValue) && itemTypeValue != null)
            {
                if (int.TryParse(itemTypeValue.ToString(), out var it))
                    _itemType = it;
            }
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                _allPriceLevels = PriceLevel.List.ToList();
                FilterPriceLevels(string.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading price levels: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        public void FilterPriceLevels(string searchText)
        {
            PriceLevels.Clear();

            var filtered = string.IsNullOrWhiteSpace(searchText)
                ? _allPriceLevels
                : _allPriceLevels.Where(x => x.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true).ToList();

            foreach (var priceLevel in filtered)
            {
                PriceLevels.Add(new PriceLevelViewModel
                {
                    Id = priceLevel.Id,
                    Name = priceLevel.Name ?? $"Price Level {priceLevel.Id}"
                });
            }
        }

        [RelayCommand]
        private async Task SelectPriceLevel(PriceLevelViewModel priceLevel)
        {
            if (priceLevel != null)
            {
                // If we have orderId and productId, update the product price for this order
                if (_orderId.HasValue && _productId.HasValue)
                {
                    var order = Order.Orders.FirstOrDefault(x => x.OrderId == _orderId.Value);
                    var product = Product.Products.FirstOrDefault(x => x.ProductId == _productId.Value);
                    
                    if (order != null && product != null)
                    {
                        // Update the client's price level if needed
                        // This is a simplified implementation - the actual logic may vary
                        if (order.Client != null)
                        {
                            order.Client.PriceLevel = priceLevel.Id;
                            // Recalculate prices for this product in the order
                            var details = order.Details.Where(x => x.Product.ProductId == product.ProductId).ToList();
                            foreach (var detail in details)
                            {
                                var newPrice = Product.GetPriceForProduct(product, order, _itemType > 0, _itemType == 1);
                                detail.Price = newPrice;
                                detail.ExpectedPrice = newPrice;
                            }
                            order.Save();
                        }
                    }
                }

                var result = new Dictionary<string, object>
                {
                    { "priceLevelId", priceLevel.Id },
                    { "priceLevelName", priceLevel.Name }
                };

                await Shell.Current.GoToAsync("..", result);
            }
        }
    }

    public partial class PriceLevelViewModel : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _name = string.Empty;
    }
}

