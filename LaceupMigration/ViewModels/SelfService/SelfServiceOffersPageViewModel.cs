using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using LaceupMigration.Services;
using LaceupMigration;

namespace LaceupMigration.ViewModels.SelfService
{
    public partial class SelfServiceOffersPageViewModel : ObservableObject, IQueryAttributable
    {
        private const string MixMatchPrefix = "MIX & MATCH: ";
        private readonly IDialogService _dialogService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private readonly MainPageViewModel _mainPageViewModel;
        private Order _order;
        private List<OfferItemViewModel> _allOffers = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _title = "Offers";

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private bool _showPrices = true;

        public ObservableCollection<OfferItemViewModel> Offers { get; } = new();

        public SelfServiceOffersPageViewModel(IDialogService dialogService, AdvancedOptionsService advancedOptionsService, MainPageViewModel mainPageViewModel)
        {
            _dialogService = dialogService;
            _advancedOptionsService = advancedOptionsService;
            _mainPageViewModel = mainPageViewModel;
            ShowPrices = !Config.HidePriceInSelfService;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue("orderId", out var orderIdObj) && int.TryParse(orderIdObj?.ToString(), out var orderId))
            {
                _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
                if (_order?.Client != null)
                    ClientName = (_order.Client.ClientName ?? string.Empty).ToUpperInvariant();
            }
            LoadOffers();
        }

        public void OnAppearing()
        {
            ShowPrices = !Config.HidePriceInSelfService;
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterOffers();
        }

        /// <summary>
        /// Matches Xamarin OffersActivity.RefreshList(): get offers, ensure Product set, expand MIX & MATCH (Discount/DiscountQty/DiscountAmount) by product in same DiscountCategoryId, then build display list.
        /// </summary>
        private void LoadOffers()
        {
            _allOffers.Clear();
            Offers.Clear();
            if (_order?.Client == null)
                return;

            var client = _order.Client;
            var offers = Offer.GetOffersVisibleToClient(client, includeDiscount: true);
            if (offers == null)
                return;

            // Work on a mutable list (Xamarin uses same list and adds/removes)
            var offersList = offers.OrderBy(x => x.Product?.Name ?? "").ToList();

            // Ensure Product is set on each offer (Xamarin lines 63-65)
            foreach (var offer in offersList)
            {
                if (offer.Product == null)
                    offer.Product = Product.Find(offer.ProductId);
            }

            // Expand MIX & MATCH: for Discount, DiscountQty, DiscountAmount, create one offer per product in the same DiscountCategoryId, then remove the category-level offer (Xamarin 68-104)
            var mixAndMatchOffers = offersList.Where(x => x.Type == OfferType.Discount || x.Type == OfferType.DiscountQty || x.Type == OfferType.DiscountAmount).ToList();
            var offersToRemove = new List<Offer>();
            foreach (var offer in mixAndMatchOffers)
            {
                if (offer.Product == null)
                    continue;
                var productsInCategory = Product.Products.Where(x => x.DiscountCategoryId == offer.Product.DiscountCategoryId).ToList();
                bool shouldRemove = false;
                foreach (var p in productsInCategory)
                {
                    offersList.Add(new Offer
                    {
                        ProductId = p.ProductId,
                        Product = p,
                        ClienBased = offer.ClienBased,
                        FreeQty = offer.FreeQty,
                        FromDate = offer.FromDate,
                        ExtraFields = offer.ExtraFields,
                        ToDate = offer.ToDate,
                        MinimunQty = offer.MinimunQty,
                        Price = offer.Price,
                        Type = offer.Type,
                        UnitOfMeasureId = offer.UnitOfMeasureId
                    });
                    shouldRemove = true;
                }
                if (shouldRemove && !offersToRemove.Contains(offer))
                    offersToRemove.Add(offer);
            }
            foreach (var o in offersToRemove)
                offersList.Remove(o);

            // Re-sort (Xamarin 106)
            offersList = offersList.OrderBy(x => x.Product?.Name ?? "").ToList();

            // Optional: only show offers for products available to client (Xamarin 113-134)
            if (Config.PreviewOfferPriceInAddItem)
            {
                try
                {
                    offersList = offersList.OrderBy(x => x.ToDate).ToList();
                    var productsForClient = Product.GetProductListForClient(client, 0, string.Empty);
                    var newList = new List<Offer>();
                    foreach (var o in offersList)
                    {
                        if (o.Product != null && productsForClient.Contains(o.Product))
                            newList.Add(o);
                    }
                    offersList = newList;
                }
                catch { /* match Xamarin: silent catch */ }
            }

            // Build display items
            foreach (var offer in offersList)
            {
                var product = offer.Product ?? Product.Find(offer.ProductId);
                if (product == null)
                    continue;
                var productName = product.Name ?? $"Product #{offer.ProductId}";
                var regPrice = ShowPrices ? Product.GetPriceForProduct(product, client, useOffer: false, useConfig: true) : 0d;
                var regText = ShowPrices ? $"Reg. {regPrice.ToCustomString()}" : "Reg. $0.00";
                var (discountText, offerLine1, offerLine2) = GetOfferDisplay(offer, regPrice, client);
                var startText = offer.FromDate.Year > 1 && offer.FromDate.Year < 9999 ? offer.FromDate.ToString("MM/dd/yyyy") : "-";
                var endDate = offer.ToDate;
                var endText = (endDate.Year <= 1 || endDate.Year >= 9999) ? "-" : endDate.ToString("MM/dd/yyyy");
                var isMixAndMatch = offer.Type == OfferType.Discount || offer.Type == OfferType.DiscountQty || offer.Type == OfferType.DiscountAmount;
                var title = isMixAndMatch ? MixMatchPrefix + productName : productName;

                _allOffers.Add(new OfferItemViewModel
                {
                    TitleLine1 = title,
                    TitleLine2 = "",
                    ProductCode = product.Code ?? "",
                    RegPriceText = regText,
                    DiscountText = discountText,
                    StartDateText = "Start " + startText,
                    OfferLine1 = offerLine1,
                    OfferLine2 = offerLine2,
                    EndDateText = "End: " + endText,
                    ShowPrice = ShowPrices
                });
            }

            // Search filter is applied in FilterOffers (Xamarin 107-110: filter by Product.Name and Product.Code)
            FilterOffers();
        }

        private (string DiscountText, string OfferLine1, string OfferLine2) GetOfferDisplay(Offer offer, double regPrice, Client client)
        {
            if (!ShowPrices)
                return ("Discount: $0.00", "Offer: $0.00", "");

            switch (offer.Type)
            {
                case OfferType.NewItem:
                    return ($"Discount: $0.00", "Price: " + (offer.Product != null ? Product.GetPriceForProduct(offer.Product, client, false).ToCustomString() : "0"), "");
                case OfferType.Price:
                    var offerPrice = offer.Price;
                    var disc = regPrice - offerPrice;
                    var discStr = disc >= 0 ? disc.ToCustomString() : $"({(-disc).ToCustomString()})";
                    return ($"Discount: {discStr}", $"Offer: {offerPrice.ToCustomString()}", "");
                case OfferType.QtyPrice:
                    var rp = offer.Product != null ? Product.GetPriceForProduct(offer.Product, client, false) : 0;
                    var d = rp - offer.Price;
                    return ($"Discount: {d.ToCustomString()}", "Price: " + offer.Price.ToCustomString(), "");
                case OfferType.QtyQty:
                    var pp = offer.Product != null ? Product.GetPriceForProduct(offer.Product, client, false) : 0d;
                    var qty = offer.MinimunQty + offer.FreeQty;
                    var ppr = qty * pp;
                    var discQty = offer.MinimunQty * pp;
                    var remaining = ppr - discQty;
                    return ($"Discount: {remaining.ToCustomString()}", $"Buy Qty: {offer.MinimunQty:F0}", $"Get Free: {offer.FreeQty:F0}");
                case OfferType.QtyQtyPrice:
                    var qqpR = offer.Product != null ? Product.GetPriceForProduct(offer.Product, client, false) : 0d;
                    var qqpD = qqpR - offer.Price;
                    return ($"Discount: {qqpD.ToCustomString()}", $"Get {offer.FreeQty:F0}", "At " + offer.Price.ToCustomString());
                case OfferType.DiscountQty:
                    double discountQtyVal = 0;
                    var productsDq = Product.Products.Where(x => x.DiscountCategoryId == (offer.Product?.DiscountCategoryId ?? 0)).ToList();
                    var testP = productsDq.FirstOrDefault();
                    if (testP != null)
                    {
                        var ppD = Product.GetPriceForProduct(testP, client, false);
                        var qtyD = offer.MinimunQty + offer.FreeQty;
                        var pprD = qtyD * ppD;
                        var discD = offer.MinimunQty * ppD;
                        discountQtyVal = pprD - discD;
                    }
                    return ($"Discount: {discountQtyVal.ToCustomString()}", $"Buy Qty: {offer.MinimunQty:F0}", $"Get Free: {offer.FreeQty:F0}");
                case OfferType.DiscountAmount:
                    double discountAmtVal = 0;
                    var productsDa = Product.Products.Where(x => x.DiscountCategoryId == (offer.Product?.DiscountCategoryId ?? 0)).ToList();
                    var testP1 = productsDa.FirstOrDefault();
                    if (testP1 != null)
                    {
                        var daR = Product.GetPriceForProduct(testP1, client, false);
                        discountAmtVal = daR - offer.Price;
                    }
                    return ($"Discount: {discountAmtVal.ToCustomString()}", $"Buy Qty: {offer.MinimunQty:F0}", "Price: " + offer.Price.ToCustomString());
                case OfferType.Discount:
                    double discountVal = 0;
                    var productsD = Product.Products.Where(x => x.DiscountCategoryId == (offer.Product?.DiscountCategoryId ?? 0)).ToList();
                    var testP2 = productsD.FirstOrDefault();
                    if (testP2 != null)
                    {
                        var ddR = Product.GetPriceForProduct(testP2, client, false);
                        discountVal = ddR - offer.Price;
                    }
                    return ($"Discount: {discountVal.ToCustomString()}", "Offer: " + offer.Price.ToCustomString(), "");
                default:
                    return ($"Discount: {offer.Price.ToCustomString()}", $"Offer: {offer.Price.ToCustomString()}", "");
            }
        }

        private void FilterOffers()
        {
            Offers.Clear();
            var search = (SearchText ?? "").Trim().ToLowerInvariant();
            if (string.IsNullOrEmpty(search))
            {
                foreach (var item in _allOffers)
                    Offers.Add(item);
                return;
            }
            // Xamarin filters by Product.Name and Product.Code
            foreach (var item in _allOffers.Where(x =>
                x.TitleLine1.ToLowerInvariant().Contains(search) ||
                (x.ProductCode ?? "").ToLowerInvariant().Contains(search)))
                Offers.Add(item);
        }

        [RelayCommand]
        private async Task ShowToolbarMenuAsync()
        {
            var options = new List<string> { "Sync Data From Server", "Advanced Options" };
            if (Client.Clients.Count == 1)
                options.Add("Sign Out");
            var choice = await _dialogService.ShowActionSheetAsync("Menu", null, "Cancel", options.ToArray());
            if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;
            if (choice == "Sync Data From Server")
                await _mainPageViewModel.SyncDataFromMenuAsync();
            else if (choice == "Advanced Options")
                await _advancedOptionsService.ShowAdvancedOptionsAsync();
            else if (choice == "Sign Out")
                await _mainPageViewModel.SignOutFromSelfServiceAsync();
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await Helpers.NavigationHelper.GoBackFromAsync("offers");
        }
    }

    public partial class OfferItemViewModel : ObservableObject
    {
        public string TitleLine1 { get; set; } = string.Empty;
        public string TitleLine2 { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public string RegPriceText { get; set; } = string.Empty;
        public string DiscountText { get; set; } = string.Empty;
        public string StartDateText { get; set; } = string.Empty;
        public string OfferLine1 { get; set; } = string.Empty;
        public string OfferLine2 { get; set; } = string.Empty;
        public string EndDateText { get; set; } = string.Empty;
        public bool ShowPrice { get; set; } = true;
    }
}
