using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using LaceupMigration.Services;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class OrderCreditPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private Order? _order;
        private bool _asPresale;
        private bool _initialized;
        private int _lastDetailCount = 0;
        private int? _lastDetailId = null;
        private bool _isReturn;
        /// <summary>True when navigated from PreviouslyOrderedTemplatePage (Add Credit). When set, empty order is not deleted on Done.</summary>
        private bool _fromOneDoc;

        public ObservableCollection<OrderCreditLineItemViewModel> LineItems { get; } = new();

        [ObservableProperty] private string _clientName = string.Empty;

        [ObservableProperty] private string _companyText = string.Empty;

        [ObservableProperty] private bool _showCompany = false;

        [ObservableProperty] private string _linesText = "Lines: 0";

        [ObservableProperty] private string _qtySoldText = "Qty Sold: 0";

        [ObservableProperty] private string _subtotalText = "Subtotal: $0.00";

        [ObservableProperty] private string _discountText = "Discount: $0.00";

        [ObservableProperty] private string _taxText = "Tax: $0.00";

        [ObservableProperty] private string _totalText = "Total: $0.00";

        [ObservableProperty] private string _termsText = "Terms: ";

        [ObservableProperty] private string _sortByText = "Sort By: Product Name";

        [ObservableProperty] private bool _showTotals = true;

        [ObservableProperty] private bool _showDiscount = true;

        [ObservableProperty] private bool _canEdit = true;

        [ObservableProperty] private bool _showAddProduct = true;

        [ObservableProperty] private bool _showViewCategories = true;

        [ObservableProperty] private bool _showSearch = true;

        [ObservableProperty] private string _doneButtonText = "Done";

        public OrderCreditPageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
            ShowTotals = !Config.HidePriceInTransaction;
            ShowDiscount = Config.AllowDiscount;
        }

        public async Task InitializeAsync(int orderId, bool asPresale, bool fromOneDoc = false)
        {
            if (_initialized && _order?.OrderId == orderId)
            {
                await RefreshAsync();
                return;
            }

            _order = Order.Orders.FirstOrDefault(x => x.OrderId == orderId);
            if (_order == null)
            {
                // await _dialogService.ShowAlertAsync("Order not found.", "Error");
                return;
            }

            _asPresale = asPresale;
            _fromOneDoc = fromOneDoc;
            _isReturn = (Config.UseReturnInvoice || (Config.UseReturnOrder && _order.OrderType != OrderType.Order));
            _initialized = true;
            _lastDetailCount = _order.Details.Count;
            LoadOrderData();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized) return;

            // Equivalent to OnStart - Set order location
            if (_order != null)
            {
                _order.Latitude = Config.LastLatitude;
                _order.Longitude = Config.LastLongitude;
            }

            // Equivalent to OnResume/OnNewIntent - Check if items were added
            if (_order != null)
            {
                if (_order.Details.Count != _lastDetailCount)
                {
                    LoadOrderData();
                    if (_lastDetailId.HasValue)
                    {
                        var lastDetail = _order.Details.FirstOrDefault(x => x.OrderDetailId == _lastDetailId.Value);
                        if (lastDetail != null)
                        {
                            // TODO: Scroll to this detail
                            _lastDetailId = null;
                        }
                    }
                    _lastDetailCount = _order.Details.Count;
                }
            }

            // Update UI state
            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            if (_order == null) return;

            // Xamarin PreviouslyOrderedTemplateActivity logic:
            // If !AsPresale && (Finished || Voided), disable all modifications (only Print allowed)
            bool isReadOnly = !_order.AsPresale && (_order.Finished || _order.Voided);
            
            if (isReadOnly)
            {
                CanEdit = false;
                ShowAddProduct = false;
                ShowViewCategories = false;
                ShowSearch = false;
            }
            else
            {
                // Update button states based on order state
                CanEdit = !_order.Locked() && !_order.Dexed && !_order.Finished && !_order.Voided;
                ShowAddProduct = CanEdit;
                ShowViewCategories = CanEdit && !_order.Dexed;
                ShowSearch = CanEdit;

                if (_order.Dexed || _order.Voided || _order.Finished)
                {
                    CanEdit = false;
                    ShowAddProduct = false;
                    ShowViewCategories = false;
                    ShowSearch = false;
                }
            }

            // Handle LoadNextActivity - process pending activities
            // TODO: Implement LoadNextActivity if needed

            // Refresh order data
            LoadOrderData();
            await Task.CompletedTask;
        }

        private void LoadOrderData()
        {
            if (_order == null) return;

            ClientName = _order.Client?.ClientName ?? "Unknown Client";
            TermsText = "Terms: " + _order.Term;

            // Company info
            if (!string.IsNullOrEmpty(_order.CompanyName))
            {
                CompanyText = $"Company: {_order.CompanyName}";
                ShowCompany = true;
            }
            else
            {
                ShowCompany = false;
            }

            // Xamarin PreviouslyOrderedTemplateActivity logic:
            // If !AsPresale && (Finished || Voided), disable all modifications (only Print allowed)
            bool isReadOnly = !_order.AsPresale && (_order.Finished || _order.Voided);
            
            if (isReadOnly)
            {
                CanEdit = false;
                ShowAddProduct = false;
                ShowViewCategories = false;
                ShowSearch = false;
            }
            else
            {
                CanEdit = !_order.Locked() && !_order.Dexed && !_order.Finished && !_order.Voided;
                ShowAddProduct = CanEdit;
                ShowViewCategories = CanEdit && !_order.Dexed;
                ShowSearch = CanEdit;

                if (_order.Dexed || _order.Voided || _order.Finished)
                {
                    CanEdit = false;
                    ShowAddProduct = false;
                    ShowViewCategories = false;
                    ShowSearch = false;
                }
            }

            if (_asPresale && _order.OrderType == OrderType.Credit)
            {
                DoneButtonText = "Send Order";
            }
            else
            {
                DoneButtonText = "Done";
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            if (_order == null) return;

            var details = _order.Details;
            // Lines count shows all details (matching Xamarin)
            LinesText = $"Lines: {details.Count}";

            // Qty Sold shows only non-credit items (matching Xamarin)
            var qtySold = details.Where(x => !x.IsCredit).Sum(x => x.Qty);
            QtySoldText = $"Qty Sold: {qtySold}";

            var subtotal = _order.OrderTotalCost();
            SubtotalText = $"Subtotal: {subtotal.ToCustomString()}";

            var discount = Math.Abs(_order.CalculateDiscount());
            DiscountText = $"Discount: {discount.ToCustomString()}";

            var tax = _order.CalculateTax();
            TaxText = $"Tax: {tax.ToCustomString()}";

            var total = _order.OrderTotalCost();
            TotalText = $"Total: {total.ToCustomString()}";

            // Load line items - only credit items (matching Xamarin's SyncLinesWithOrder)
            // Xamarin shows only items with OrderDetail != null if order has details
            LineItems.Clear();
            var creditDetails = details.Where(x => x.IsCredit).ToList();
            
            // If order has details, only show items that have OrderDetail (matching Xamarin logic)
            if (details.Count > 0)
            {
                creditDetails = creditDetails.Where(x => x != null).ToList();
            }
            
            var sortedDetails = SortDetails.SortedDetails(creditDetails).ToList();

            foreach (var detail in sortedDetails)
            {
                var lineItem = CreateLineItemViewModel(detail);
                LineItems.Add(lineItem);
            }
        }

        private OrderCreditLineItemViewModel CreateLineItemViewModel(OrderDetail detail)
        {
            if (_order == null || detail == null || detail.Product == null)
                return new OrderCreditLineItemViewModel();

            var qtyText = detail.Product.SoldByWeight && detail.Weight > 0
                ? $"Qty: {detail.Qty} (Weight: {detail.Weight})"
                : $"Qty: {detail.Qty}";

            // On hand: use GetInventory(AsPresale, false) so when !AsPresale we show truck inventory (same as PreviouslyOrderedTemplatePage)
            var onHand = detail.Product.GetInventory(_order.AsPresale, false);
            var onHandText = $"OH:{onHand:F0}";
            var onHandColor = onHand <= 0 ? Colors.Red : Colors.Orange;

            // List price
            var listPrice = Product.GetPriceForProduct(detail.Product, _order, true, false);
            var listPriceText = $"List Price: ({listPrice.ToCustomString()})";

            // Current price
            var priceText = detail.Price > 0 ? $"Price:({detail.Price.ToCustomString()})" : string.Empty;

            // Total for this line
            var total = detail.Qty * detail.Price;
            var totalText = $"Total:({total.ToCustomString()})";

            // Type (Return/Dump)
            var typeText = string.Empty;
            var typeColor = Colors.Transparent;
            var showType = false;

            if (detail.IsCredit)
            {
                typeText = detail.Damaged ? "Dump" : "Return";
                typeColor = Colors.Orange;
                showType = true;
            }

            // UoM (e.g. Case, Each, Dozen) so multiple lines for same product are distinguishable
            var uomText = detail.UnitOfMeasure != null ? detail.UnitOfMeasure.Name : string.Empty;

            // Quantity button text
            var qtyButtonText = detail.Qty > 0 ? detail.Qty.ToString("F0") : "+";

            return new OrderCreditLineItemViewModel
            {
                Detail = detail,
                ProductName = detail.Product.Name,
                OnHandText = onHandText,
                OnHandColor = onHandColor,
                ListPriceText = listPriceText,
                QtyText = qtyText,
                PriceText = priceText,
                UomText = uomText,
                TotalText = totalText,
                TypeText = typeText,
                TypeColor = typeColor,
                ShowType = showType,
                QuantityButtonText = qtyButtonText
            };
        }

        [RelayCommand]
        private async Task LineItemSelectedAsync(OrderCreditLineItemViewModel? item)
        {
            if (item == null || item.Detail == null) return;

            // Navigate to AddItemPage to edit this credit item (use orderDetail param so correct detail is loaded)
            await Shell.Current.GoToAsync($"additem?orderId={_order?.OrderId}&orderDetail={item.Detail.OrderDetailId}&asCreditItem=1");
        }

        [RelayCommand]
        private async Task EditLineItemAsync(OrderCreditLineItemViewModel? item)
        {
            if (item == null || item.Detail == null || _order == null || !CanEdit) return;

            // Use same RestOfTheAddDialog as PreviouslyOrderedTemplatePage for editing qty (lot, weight, price, comments, etc.)
            var existingDetail = item.Detail;
            var result = await _dialogService.ShowRestOfTheAddDialogAsync(
                item.Detail.Product,
                _order,
                existingDetail,
                isCredit: true,
                isDamaged: existingDetail.Damaged,
                isDelivery: _order.IsDelivery);

            if (result.Cancelled)
                return;

            // Handle qty == 0 - delete the detail
            if (result.Qty == 0)
            {
                _order.DeleteDetail(existingDetail);
                _order.Save();
                LoadOrderData();
                return;
            }

            // Update existing detail with dialog result (same as PreviouslyOrderedTemplatePage)
            existingDetail.Qty = result.Qty;
            existingDetail.Weight = result.Weight;
            existingDetail.Lot = result.Lot;
            if (result.LotExpiration.HasValue)
                existingDetail.LotExpiration = result.LotExpiration.Value;
            existingDetail.Comments = result.Comments;
            existingDetail.Price = result.Price;
            existingDetail.UnitOfMeasure = result.SelectedUoM;
            existingDetail.IsFreeItem = result.IsFreeItem;
            existingDetail.ReasonId = result.ReasonId;
            existingDetail.Discount = result.Discount;
            existingDetail.DiscountType = result.DiscountType;
            if (result.PriceLevelSelected > 0)
            {
                existingDetail.ExtraFields = UDFHelper.SyncSingleUDF("priceLevelSelected", result.PriceLevelSelected.ToString(), existingDetail.ExtraFields);
            }

            OrderDetailMergeHelper.TryMergeDuplicateDetail(_order, existingDetail);
            OrderDetail.UpdateRelated(existingDetail, _order);
            _order.RecalculateDiscounts();
            _order.Save();
            LoadOrderData();
        }

        [RelayCommand]
        private async Task AddProductAsync()
        {
            if (_order == null) return;

            // Equivalent to CreditSectionActivityLayoutProd_Click
            OrderDetail? lastDetail = null;
            
            if (Config.CatalogQuickAdd)
            {
                lastDetail = _order.Details.Where(x => x.IsCredit).OrderByDescending(x => x.OrderDetailId).FirstOrDefault();
            }
            else
            {
                lastDetail = _order.Details.Where(x => x.IsCredit).OrderByDescending(x => x.OrderDetailId).FirstOrDefault();
            }

            if (lastDetail != null)
            {
                var route = $"productcatalog?orderId={_order.OrderId}&categoryId={lastDetail.Product.CategoryId}";
                route += "&asCreditItem=1";
                
                if ((Config.UseReturnInvoice || (Config.UseReturnOrder && _order.OrderType != OrderType.Order)) && !lastDetail.Damaged)
                {
                    route += "&asReturnItem=1";
                }
                
                route += "&viaFullCategory=0";
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                await ViewCategoriesAsync();
            }
        }

        [RelayCommand]
        private async Task ViewCategoriesAsync()
        {
            if (_order == null) return;

            // Equivalent to ShowCategoryActivity
            // Pass comingFrom=Credit so ProductCatalog knows to prompt for Credit/Return
            if (Category.Categories.Count == 1)
            {
                var category = Category.Categories.FirstOrDefault();
                if (category != null)
                {
                    var route = $"fullcategory?orderId={_order.OrderId}&categoryId={category.CategoryId}&asCreditItem=1&comingFrom=Credit";
                    if ((Config.UseReturnInvoice || (Config.UseReturnOrder && _order.OrderType != OrderType.Order)) && _isReturn)
                    {
                        route += "&asReturnItem=1";
                    }
                    await Shell.Current.GoToAsync(route);
                }
            }
            else
            {
                var route = $"fullcategory?orderId={_order.OrderId}&clientId={_order.Client.ClientId}&asCreditItem=1&comingFrom=Credit";
                if ((Config.UseReturnInvoice || (Config.UseReturnOrder && _order.OrderType != OrderType.Order)) && _isReturn)
                {
                    route += "&asReturnItem=1";
                }
                await Shell.Current.GoToAsync(route);
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            if (_order == null) return;

            // Equivalent to CreditSectionActivityLayoutSearch_Click
            var searchTerm = await _dialogService.ShowPromptAsync("Enter Product Name", "Search", "OK", "Cancel", "Product name, UPC, SKU, or code");
            
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            searchTerm = searchTerm.ToLowerInvariant().Trim();

            // Search products
            IEnumerable<Product> list = Product.Products;
            if (_order.Client.CategoryId != 0)
                list = ClientCategoryProducts.Find(_order.Client.CategoryId).Products;

            var products = list.Where(x => (
                x.Name.ToLowerInvariant().IndexOf(searchTerm) != -1 ||
                x.Sku.ToLowerInvariant().Contains(searchTerm) ||
                x.Upc.ToLowerInvariant().Contains(searchTerm) ||
                x.Code.ToLowerInvariant().Contains(searchTerm) ||
                x.Description.ToLowerInvariant().Contains(searchTerm)
            ) && (x.CategoryId != 0)).ToList();

            if (Config.ButlerCustomization && _order.Client.PriceLevel > 0)
            {
                var productPrices = ProductPrice.Pricelist.Where(x => x.PriceLevelId == _order.Client.PriceLevel).Select(x => x.ProductId).Distinct().ToList();
                products = products.Where(x => productPrices.Contains(x.ProductId)).ToList();
            }

            if (products.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No products found.", "Search");
            }
            else if (products.Count == 1)
            {
                var product = products.First();
                
                // Check availability
                if (!string.IsNullOrEmpty(product.NonVisibleExtraFieldsAsString))
                {
                    var available = UDFHelper.GetSingleUDF("AvailableIn", product.NonVisibleExtraFieldsAsString);
                    if (!string.IsNullOrEmpty(available))
                    {
                        if (available.ToLower() == "none" || !available.ToLower().Contains("credit"))
                        {
                            await _dialogService.ShowAlertAsync("Product unavailable.", "Warning");
                            return;
                        }
                    }
                }

                // Navigate to FullCategoryPage with search
                var route = $"fullcategory?orderId={_order.OrderId}&productSearch={searchTerm}&comingFromSearch=yes&asCreditItem=1&comingFrom=Credit";
                if ((Config.UseReturnInvoice || (Config.UseReturnOrder && _order.OrderType != OrderType.Order)) && _isReturn)
                {
                    route += "&asReturnItem=1";
                }
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                var route = $"fullcategory?orderId={_order.OrderId}&productSearch={searchTerm}&comingFromSearch=yes&asCreditItem=1&comingFrom=Credit";
                if ((Config.UseReturnInvoice || (Config.UseReturnOrder && _order.OrderType != OrderType.Order)) && _isReturn)
                {
                    route += "&asReturnItem=1";
                }
                await Shell.Current.GoToAsync(route);
            }
        }

        [RelayCommand]
        public async Task DoneAsync()
        {
            if (_order == null) return;

            // When coming from PreviouslyOrderedTemplatePage (Add Credit), never delete empty order
            if (_order.Details.Count == 0 && _fromOneDoc)
            {
                Helpers.NavigationHelper.RemoveNavigationState("ordercredit");
                await Shell.Current.GoToAsync("..");
                return;
            }

            if (_order.Details.Count == 0)
            {
                var route = RouteEx.Routes.FirstOrDefault(x => x.Order != null && x.Order.OrderId == _order.OrderId);
                if (string.IsNullOrEmpty(_order.PrintedOrderId) && route == null)
                {
                    _order.Delete();
                    await Shell.Current.GoToAsync("..");
                    return;
                }
            }

            // Check if order is empty - if so, handle it and return (matching Xamarin logic)
            bool isEmpty = _order.Details.Count == 0 || 
                (_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem);

            if (isEmpty)
            {
                // Handle empty order - this will either delete it or show void dialog
                var canNavigate = await FinalizeOrderAsync();
                if (canNavigate)
                {
                    // [ACTIVITY STATE]: Remove state when navigating away
                    Helpers.NavigationHelper.RemoveNavigationState("ordercredit");
                    
                    await Shell.Current.GoToAsync("..");
                }
                return; // Don't show the 3-option dialog for empty orders
            }

            // Check if order is presale and show dialog with 3 options (only for non-empty orders)
            if (_order.AsPresale && !_fromOneDoc)
            {
                // Show action options dialog (matching Xamarin PreviouslyOrderedTemplateActivity logic)
                var options = new[]
                {
                    "Send Order",
                    "Save Order To Send Later",
                    "Stay In The Order"
                };

                var choice = await _dialogService.ShowActionSheetAsync("Action Options", "", "Cancel", options);
                
                if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
                    return; // User cancelled, stay in order

                switch (choice)
                {
                    case "Send Order":
                        // Call SendOrderAsync which handles validation and sending
                        await SendOrderAsync();
                        break;
                    case "Save Order To Send Later":
                        // Continue after alert - finalize order and navigate
                        var canNavigate = await FinalizeOrderAsync();
                        if (canNavigate)
                        {
                            // [ACTIVITY STATE]: Remove state when navigating away
                            Helpers.NavigationHelper.RemoveNavigationState("ordercredit");
                            
                            await Shell.Current.GoToAsync("..");
                        }
                        break;
                    case "Stay In The Order":
                        // Do nothing, stay in the order
                        return;
                }
            }
            else
            {
                // Non-presale order - use normal finalization logic
                var canNavigate = await FinalizeOrderAsync();
                if (canNavigate)
                {
                    // [ACTIVITY STATE]: Remove state when navigating away
                    Helpers.NavigationHelper.RemoveNavigationState("ordercredit");
                    
                    await Shell.Current.GoToAsync("..");
                }
            }
        }

        public async Task<bool> FinalizeOrderAsync()
        {
            if (_order == null)
                return true;

            // Check if order is locked or finished
            if (_order.OrderType == OrderType.Order || _order.OrderType == OrderType.Bill || _order.Locked())
            {
                return true; // Allow navigation
            }

            // Check if order is empty
            if (_order.Details.Count == 0)
            {
                // When coming from PreviouslyOrderedTemplatePage (Add Credit / fromOneDoc), never delete or void the empty order
                if (_fromOneDoc)
                    return true; // Allow navigation without touching the order

                if (_order.AsPresale)
                {
                    UpdateRoute(false);

                    var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
                    if (batch != null)
                    {
                        Logger.CreateLog($"Batch with id={batch.Id} DELETED (1 order without details)");
                        batch.Delete();
                    }

                    _order.Delete();
                    return true; // Allow navigation
                }
                else
                {
                    var route = RouteEx.Routes.FirstOrDefault(x => x.Order != null && x.Order.OrderId == _order.OrderId);
                    if (string.IsNullOrEmpty(_order.PrintedOrderId) && route == null)
                    {
                        _order.Delete();
                        return true; // Allow navigation
                    }
                    else
                    {
                        var result = await _dialogService.ShowConfirmAsync(
                            "You have to set all quantities to zero. Do you want to void this order?",
                            "Alert",
                            "Yes",
                            "No");
                        if (result)
                        {
                            _order.Finished = true;
                            _order.Void();
                            _order.Save();
                            return true; // Allow navigation
                        }
                        return false; // Don't navigate
                    }
                }
            }

            // Check mandatory ship date
            if (_order.AsPresale && Config.ShipDateIsMandatory && _order.ShipDate.Year == 1)
            {
                await _dialogService.ShowAlertAsync("Please select ship date.", "Alert");
                return false; // Don't navigate
            }

            // Check if order must be sent
            if (_order.AsPresale && Config.SendOrderIsMandatory)
            {
                await _dialogService.ShowAlertAsync("Order must be sent.", "Alert");
                return false; // Don't navigate
            }

            // Set end date
            if (_order.EndDate == DateTime.MinValue)
            {
                _order.EndDate = DateTime.Now;
                _order.Save();
            }

            // Add to session
            if (Session.session != null)
                Session.session.AddDetailFromOrder(_order);

            // Update route
            if (_order.AsPresale)
            {
                UpdateRoute(true);

                if (Config.GeneratePresaleNumber && string.IsNullOrEmpty(_order.PrintedOrderId))
                {
                    _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);
                    _order.Save();
                }
            }

            return true; // Allow navigation
        }

        private async Task SendOrderAsync()
        {
            if (_order == null)
                return;

            // Check if order has details
            bool isEmpty = _order.Details.Count == 0 || 
                (_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem);
            
            if (isEmpty)
            {
                await _dialogService.ShowAlertAsync("You can't send an empty order", "Alert");
                return;
            }

            // Validate presale requirements
            if (_order.AsPresale)
            {
                // Check for mandatory comments
                if (string.IsNullOrEmpty(_order.Comments) && Config.PresaleCommMandatory)
                {
                    await _dialogService.ShowAlertAsync("Please provide a comment.", "Alert");
                    return;
                }

                // Check for signature name
                if (Config.SignatureNameRequired && string.IsNullOrEmpty(_order.SignatureName))
                {
                    await _dialogService.ShowAlertAsync("Signature name is required.", "Alert");
                    return;
                }

                // Check for mandatory images
                if (Config.CaptureImages && Config.ImageInOrderMandatory && _order.ImageList.Count <= 0)
                {
                    await _dialogService.ShowAlertAsync("Order image is mandatory to send presale order.", "Warning");
                    return;
                }

                // Check if ship date is locked
                if (Config.CheckIfShipdateLocked)
                {
                    var lockedDates = new List<DateTime>();
                    if (!DataProvider.CheckIfShipdateIsValid(new List<DateTime>() { _order.ShipDate }, ref lockedDates))
                    {
                        var sb = string.Empty;
                        foreach (var l in lockedDates)
                            sb += '\n' + l.Date.ToShortDateString();
                        await _dialogService.ShowAlertAsync("The selected date is currently locked. Please select a different shipdate", "Alert");
                        return;
                    }
                }
            }

            // Show confirmation dialog
            var confirm = await _dialogService.ShowConfirmAsync(
                "Continue sending credit?",
                "Warning",
                "Yes",
                "No");

            if (!confirm)
                return;

            // Handle ship date if needed
            if (Config.PresaleShipDate && _order.ShipDate.Year == 1)
            {
                var selectedDate = await _dialogService.ShowDatePickerAsync("Set Ship Date", DateTime.Now, DateTime.Now, null);
                if (selectedDate.HasValue)
                {
                    _order.ShipDate = selectedDate.Value;
                    _order.Save();
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Please set ship date before sending.", "Alert");
                    return;
                }
            }
            else if (Config.ShipDateIsMandatory && _order.ShipDate.Year == 1)
            {
                await _dialogService.ShowAlertAsync("Please select ship date.", "Alert");
                return;
            }

            // Send the order
            await SendItAsync();
        }

        private async Task SendItAsync()
        {
            if (_order == null)
                return;

            try
            {
                // Recalculate discounts if needed
                if (OrderDiscount.HasDiscounts)
                {
                    _order.RecalculateDiscounts();

                    if (!Config.CalculateOffersAutomatically)
                        _order.VerifyDiscount();
                }

                // Set end date if not set
                if (_order.EndDate == DateTime.MinValue)
                {
                    _order.EndDate = DateTime.Now;
                    _order.Save();
                }

                // Update route if presale
                if (_order.AsPresale)
                {
                    UpdateRoute(true);
                }

                // Generate presale number if needed
                if (_order.AsPresale && Config.GeneratePresaleNumber && string.IsNullOrEmpty(_order.PrintedOrderId))
                {
                    _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);
                }

                // Ensure batch exists
                var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
                if (batch == null)
                {
                    batch = new Batch(_order.Client);
                    batch.Client = _order.Client;
                    batch.ClockedIn = DateTime.Now;
                    batch.ClockedOut = DateTime.Now;
                    batch.Save();

                    _order.BatchId = batch.Id;
                    _order.Save();
                }

                // Send the orders
                DataProvider.SendTheOrders(new Batch[] { batch });

                // Set client Editable to false when sending presale orders (fixes Xamarin bug)
                // Only for locally created clients (ClientId <= 0)
                if (_order.AsPresale && _order.Client != null && _order.Client.ClientId <= 0)
                {
                    _order.Client.Editable = false;
                    Client.Save();
                }

                await _dialogService.ShowAlertAsync("Credit sent successfully.", "Info");

                // Navigate back
                if (_order.AsPresale)
                {
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await _appService.GoBackToMainAsync();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error sending credit.", "Alert");
            }
        }

        private void UpdateRoute(bool close)
        {
            if (!Config.CloseRouteInPresale)
                return;

            var stop = RouteEx.Routes.FirstOrDefault(x => 
                x.Date.Date == DateTime.Today && 
                x.Client != null && 
                x.Client.ClientId == _order?.Client.ClientId);
            
            if (stop != null && _order != null)
            {
                if (close)
                    stop.AddOrderToStop(_order.UniqueId);
                else
                    stop.RemoveOrderFromStop(_order.UniqueId);
            }
        }

        [RelayCommand]
        private async Task SortByAsync()
        {
            var sortOptions = new[] { "Product Name", "Product Code", "Last Visit" };
            var choice = await _dialogService.ShowActionSheetAsync("Sort By", "", "Cancel", sortOptions);
            
            if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
                return;

            // TODO: Implement sorting logic
            SortByText = $"Sort By: {choice}";
            await Task.CompletedTask;
        }

        [RelayCommand]
        async Task ShowMenuAsync()
        {
            if (_order == null) return;

            var options = BuildMenuOptions();
            if (options.Count == 0) return;

            var choice = await _dialogService.ShowActionSheetAsync("Menu", "", "Cancel",
                options.Select(o => o.Title).ToArray());
            if (string.IsNullOrWhiteSpace(choice)) return;

            var option = options.FirstOrDefault(o => o.Title == choice);
            if (option?.Action != null)
            {
                await option.Action();
            }
        }

        public List<MenuOption> BuildMenuOptions()
        {
            var options = new List<MenuOption>();

            if (_order == null) return options;

            // Xamarin PreviouslyOrderedTemplateActivity logic:
            // If !AsPresale && (Finished || Voided), only show Print option
            bool isReadOnly = !_order.AsPresale && (_order.Finished || _order.Voided);
            
            if (isReadOnly)
            {
                // Only allow Print when read-only
                if (Config.PrinterAvailable)
                {
                    options.Add(new MenuOption("Print", async () =>
                    {
                        await PrintAsync();
                    }));
                }
                return options;
            }

            var finalized = _order.Finished;
            var allowDiscount = _order.Client.UseDiscount;

            if (_asPresale)
            {
                // Presale menu items
                if (allowDiscount && !_order.Locked())
                {
                    options.Add(new MenuOption("Add Discount", async () =>
                    {
                        await _dialogService.ShowAlertAsync("Add Discount functionality is not yet fully implemented.",
                            "Info");
                        // TODO: Implement discount dialog
                    }));
                }

                if (Config.SetPO && (_order.OrderType != OrderType.Order || Config.POIsMandatory))
                {
                    options.Add(new MenuOption("Set PO", async () =>
                    {
                        var po = await _dialogService.ShowPromptAsync("Set PO", "Enter PO Number:",
                            initialValue: _order.PONumber ?? string.Empty);
                        if (!string.IsNullOrWhiteSpace(po))
                        {
                            _order.PONumber = po;
                            _order.Save();
                            await _dialogService.ShowAlertAsync("PO number set.", "Success");
                        }
                    }));
                }

                if (Config.PrinterAvailable)
                {
                    options.Add(new MenuOption("Print", async () =>
                    {
                        await PrintAsync();
                    }));
                }

                options.Add(new MenuOption("Send Order", async () =>
                {
                    await SendOrderAsync();
                }));

                if (!(_order.Client.SplitInvoices.Count > 0))
                {
                    options.Add(new MenuOption("Send by Email", async () =>
                    {
                        await SendByEmailAsync();
                    }));

                }

                // Select Driver (presale only, hidden when work order asset - matches TemplateActivity)
                var asset = UDFHelper.GetSingleUDF("workOrderAsset", _order.ExtraFields);
                if (string.IsNullOrEmpty(asset))
                {
                    options.Add(new MenuOption("Select Driver", async () =>
                    {
                        await SelectSalesmanAsync();
                    }));
                }
            }
            else
            {
                // Previously ordered menu items
                if ((Config.SetPO || Config.POIsMandatory) && !finalized)
                {
                    options.Add(new MenuOption("Set PO", async () =>
                    {
                        var po = await _dialogService.ShowPromptAsync("Set PO", "Enter PO Number:",
                            initialValue: _order.PONumber ?? string.Empty);
                        if (!string.IsNullOrWhiteSpace(po))
                        {
                            _order.PONumber = po;
                            _order.Save();
                            await _dialogService.ShowAlertAsync("PO number set.", "Success");
                        }
                    }));
                }

                if (allowDiscount && !finalized && !_order.Locked())
                {
                    options.Add(new MenuOption("Add Discount", async () =>
                    {
                        await _dialogService.ShowAlertAsync("Add Discount functionality is not yet fully implemented.",
                            "Info");
                        // TODO: Implement discount dialog
                    }));
                }

                if (!Config.LockOrderAfterPrinted)
                {
                    var isSplitClient = _order.Client.SplitInvoices.Count > 0;
                    if (!isSplitClient || _order.Finished)
                    {
                        options.Add(new MenuOption("Print", async () =>
                        {
                            await PrintAsync();
                        }));
                    }
                }

                if (!(_order.Client.SplitInvoices.Count > 0))
                {
                    options.Add(new MenuOption("Send by Email", async () =>
                    {
                        await SendByEmailAsync();
                    }));

                }
            }

            // Take Picture (order captured images)
            if (Config.CaptureImages)
            {
                options.Add(new MenuOption("Take Picture", async () =>
                {
                    await ViewCapturedImagesAsync();
                }));
            }

            // Common menu items
            options.Add(new MenuOption("Add Comments", async () =>
            {
                var comments = await _dialogService.ShowPromptAsync("Add Comments", "Enter comments:",
                    initialValue: _order.Comments ?? string.Empty);
                if (comments != null)
                {
                    _order.Comments = comments;
                    _order.Save();
                    await _dialogService.ShowAlertAsync("Comments saved.", "Success");
                }
            }));

            // Note: "Advanced Options" is already added by LaceupContentPage.GetCommonMenuOptions()
            // Don't add it here to avoid duplication

            return options;
        }

        async Task ShowAdvancedOptionsAsync()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
        }

        private async Task ViewCapturedImagesAsync()
        {
            if (_order == null)
                return;
            await Shell.Current.GoToAsync($"viewcapturedimages?orderId={_order.OrderId}");
        }

        private async Task SelectSalesmanAsync()
        {
            if (_order == null)
                return;

            // Match TemplateActivity.SelectSalesman(): Driver | DSD, InventorySiteId > 0 and != BranchSiteId
            var selectedRole = SalesmanRole.Driver | SalesmanRole.DSD;
            var salesmen = Salesman.List
                .Where(x => ((int)x.Roles & (int)selectedRole) > 0)
                .Where(x => x.InventorySiteId > 0 && x.InventorySiteId != Config.BranchSiteId)
                .OrderBy(x => x.Name)
                .ToList();

            if (salesmen.Count == 0)
            {
                await _dialogService.ShowAlertAsync("No drivers available to select.", "Select Driver");
                return;
            }

            var driverNames = salesmen.Select(x => x.Name).ToArray();
            var selectedIndex = await _dialogService.ShowSelectionAsync("Select Driver", driverNames);
            if (selectedIndex < 0 || selectedIndex >= salesmen.Count)
                return;

            _order.ExtraFields = UDFHelper.SyncSingleUDF("Salesman", salesmen[selectedIndex].Id.ToString(), _order.ExtraFields);
            _order.Save();
            await RefreshAsync();
        }

        private async Task PrintAsync()
        {
            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("No order to print.", "Alert", "OK");
                return;
            }

            try
            {
                PrinterProvider.PrintDocument((int number) =>
                {
                    if (string.IsNullOrEmpty(_order.PrintedOrderId))
                    {
                        if ((_order.AsPresale && Config.GeneratePresaleNumber) || (!_order.AsPresale && Config.GeneratePreorderNum))
                        {
                            _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);
                            _order.Save();
                        }
                    }

                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool allWent = true;

                    for (int i = 0; i < number; i++)
                    {
                        if (!printer.PrintOrder(_order, !_order.Finished))
                            allWent = false;
                    }

                    if (!allWent)
                        return "Error printing order.";
                    return string.Empty;
                });
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        private async Task SendByEmailAsync()
        {
            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("No order to send.", "Alert", "OK");
                return;
            }

            try
            {
                // Use PdfHelper to send order by email (matches Xamarin email sending)
                await PdfHelper.SendOrderByEmail(_order);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
            }
        }
    }
    
    
    public partial class OrderCreditLineItemViewModel : ObservableObject
    {
        [ObservableProperty] private string _productName = string.Empty;

        [ObservableProperty] private string _onHandText = "OH:0";

        [ObservableProperty] private Color _onHandColor = Colors.Orange;

        [ObservableProperty] private string _listPriceText = string.Empty;

        [ObservableProperty] private string _qtyText = string.Empty;

        [ObservableProperty] private string _priceText = string.Empty;

        [ObservableProperty] private string _uomText = string.Empty;

        [ObservableProperty] private string _amountText = string.Empty;

        [ObservableProperty] private string _totalText = string.Empty;

        [ObservableProperty] private string _typeText = string.Empty;

        [ObservableProperty] private Color _typeColor;

        [ObservableProperty] private bool _showType;

        [ObservableProperty] private string _quantityButtonText = "+";

        public OrderDetail Detail { get; set; } = null!;
    }
}