using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using LaceupMigration;

namespace LaceupMigration.ViewModels
{
    /// <summary>
    /// Simple product list (name + OH) for template flow, matching Xamarin FullProductListActivity.
    /// When a product is selected, navigates to NewOrderTemplateDetailsPage with orderId, productId, fromCreditTemplate.
    /// </summary>
    public partial class FullProductListPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private int? _orderId;
        private int? _categoryId;
        private int? _clientId;
        private bool _fromCreditTemplate;
        private bool _asPresale;
        private string? _productSearch;
        private List<FullProductListRowViewModel> _allRows = new();

        public ObservableCollection<FullProductListRowViewModel> Products { get; } = new();

        [ObservableProperty]
        private string _title = "Products";

        [ObservableProperty]
        private string _searchQuery = string.Empty;

        public FullProductListPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        /// <summary>Set from query and load products. Call from ApplyQueryAttributes.</summary>
        public void SetNavigationQuery(int? orderId, int? categoryId, int? clientId, bool fromCreditTemplate, bool asPresale, string? productSearch = null)
        {
            _orderId = orderId;
            _categoryId = categoryId;
            _clientId = clientId;
            _fromCreditTemplate = fromCreditTemplate;
            _asPresale = asPresale;
            _productSearch = productSearch;
            Title = _fromCreditTemplate ? "Products (Credit)" : "Products";
        }

        public void LoadProducts()
        {
            Products.Clear();
            List<Product> list;

            if (_orderId.HasValue)
            {
                var order = Order.Orders.FirstOrDefault(x => x.OrderId == _orderId.Value);
                if (order != null)
                    list = Product.GetProductListForOrder(order, _fromCreditTemplate, _categoryId ?? 0).ToList();
                else
                    list = new List<Product>();
            }
            else if (_clientId.HasValue)
            {
                var client = Client.Clients.FirstOrDefault(x => x.ClientId == _clientId.Value);
                if (client != null)
                    list = SortDetails.SortedDetails(Product.GetProductListForClient(client, _categoryId ?? 0, _productSearch ?? string.Empty)).ToList();
                else
                    list = new List<Product>();
            }
            else
            {
                list = _categoryId > 0
                    ? SortDetails.SortedDetails(Product.Products.Where(x => x.CategoryId == _categoryId).ToList()).ToList()
                    : SortDetails.SortedDetails(Product.Products.Where(x => x.CategoryId > 0).ToList()).ToList();
            }

            if (Config.SalesmanCanChangeSite && ProductAllowedSite.List.Count > 0 && Config.SalesmanSelectedSite > 0)
            {
                var allowedIds = ProductAllowedSite.List.Where(x => x.SiteId == Config.SalesmanSelectedSite).Select(x => x.ProductId).ToList();
                list = list.Where(x => allowedIds.Contains(x.ProductId)).ToList();
            }

            if (!string.IsNullOrEmpty(_productSearch))
            {
                var lower = _productSearch.ToLowerInvariant();
                list = list.Where(x => x.Name?.ToLowerInvariant().Contains(lower) == true).ToList();
            }

            _allRows.Clear();
            foreach (var p in list.Where(x => x.CategoryId > 0))
            {
                double inventory = Config.TrackInventory ? p.CurrentInventory : p.CurrentWarehouseInventory;
                if (_asPresale)
                    inventory = p.CurrentWarehouseInventory;
                _allRows.Add(new FullProductListRowViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name ?? "",
                    OnHandText = inventory.ToString("N2", CultureInfo.CurrentCulture)
                });
            }
            ApplySearchFilter();
        }

        private void ApplySearchFilter()
        {
            Products.Clear();
            var q = (SearchQuery ?? "").Trim().ToLowerInvariant();
            var source = string.IsNullOrEmpty(q) ? _allRows : _allRows.Where(x => (x.Name ?? "").ToLowerInvariant().Contains(q)).ToList();
            foreach (var row in source)
                Products.Add(row);
        }

        partial void OnSearchQueryChanged(string value)
        {
            ApplySearchFilter();
        }

        [RelayCommand]
        private async Task SelectProduct(FullProductListRowViewModel? row)
        {
            if (row == null || !_orderId.HasValue) return;
            int itemType = 0;
            if (_fromCreditTemplate)
            {
                var choice = await _dialogService.ShowActionSheetAsync("Add as", null, "Cancel", "Dump", "Return");
                if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;
                itemType = choice == "Dump" ? 1 : 2;
            }
            var route = $"newordertemplatedetails?orderId={_orderId.Value}&productId={row.ProductId}&itemType={itemType}&fromCreditTemplate={(_fromCreditTemplate ? 1 : 0)}";
            await Shell.Current.GoToAsync(route);
        }

        [RelayCommand]
        private async Task GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }

    public partial class FullProductListRowViewModel : ObservableObject
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string OnHandText { get; set; } = string.Empty;
    }
}
