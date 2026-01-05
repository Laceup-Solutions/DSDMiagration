using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class OrderDetailsPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private Order? _order;
        private bool _initialized;
        private bool _asPresale;
        private int _lastDetailCount = 0;
        private int? _lastDetailId = null;

        public ObservableCollection<OrderLineItemViewModel> LineItems { get; } = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _companyText = string.Empty;

        [ObservableProperty]
        private bool _showCompany;

        [ObservableProperty]
        private string _orderTypeText = string.Empty;

        [ObservableProperty]
        private string _orderDateText = string.Empty;

        [ObservableProperty]
        private bool _showOrderDate = true;

        [ObservableProperty]
        private string _shipDateText = string.Empty;

        [ObservableProperty]
        private bool _showShipDate;

        [ObservableProperty]
        private string _statusText = string.Empty;

        [ObservableProperty]
        private bool _showStatus;

        [ObservableProperty]
        private Color _statusColor = Colors.Transparent;

        [ObservableProperty]
        private string _subtotalText = "Subtotal: $0.00";

        [ObservableProperty]
        private string _discountText = "Discount: $0.00";

        [ObservableProperty]
        private string _taxText = "Tax: $0.00";

        [ObservableProperty]
        private string _totalText = "Total: $0.00";

        [ObservableProperty]
        private string _totalQtyText = "Total Qty: 0";

        [ObservableProperty]
        private string _linesText = "Lines: 0";

        [ObservableProperty]
        private string _qtySoldText = "Qty Sold: 0";

        [ObservableProperty]
        private string _orderAmountText = "Order: $0.00";

        [ObservableProperty]
        private string _creditAmountText = "Credit: $0.00";

        [ObservableProperty]
        private string _sortByText = "Sort By: Product Name";

        [ObservableProperty]
        private bool _showSendButton = true;

        [ObservableProperty]
        private bool _showTotals = true;

        [ObservableProperty]
        private bool _showDiscount = true;

        [ObservableProperty]
        private bool _showTax = true;

        [ObservableProperty]
        private bool _canEdit = true;

        [ObservableProperty]
        private bool _showAddProduct = true;

        [ObservableProperty]
        private bool _showViewCategories = true;

        [ObservableProperty]
        private bool _showSearch = true;

        [ObservableProperty]
        private string _commentsText = string.Empty;

        [ObservableProperty]
        private bool _showComments;

        public OrderDetailsPageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
            ShowTotals = !Config.HidePriceInTransaction;
        }

        public async Task InitializeAsync(int orderId, bool asPresale)
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
            _initialized = true;
            _lastDetailCount = _order.Details.Count;
            LoadOrderData();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

            // Equivalent to OnStart - Set order location
            if (_order != null)
            {
                _order.Latitude = DataAccess.LastLatitude;
                _order.Longitude = DataAccess.LastLongitude;
            }

            // Equivalent to OnResume/OnNewIntent - Check if items were added
            if (_order != null)
            {
                // Check if new items were added (equivalent to OnNewIntent handling)
                if (_order.Details.Count != _lastDetailCount)
                {
                    // Items were added - refresh the list
                    LoadOrderData();
                    
                    // Scroll to last added detail if we have a lastDetailId
                    if (_lastDetailId.HasValue)
                    {
                        var lastDetail = _order.Details.FirstOrDefault(x => x.OrderDetailId == _lastDetailId.Value);
                        if (lastDetail != null)
                        {
                            // TODO: Scroll to this detail in the UI
                            _lastDetailId = null; // Reset after handling
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

            // Handle LoadNextActivity - process pending activities
            // TODO: Implement LoadNextActivity if needed

            // Refresh order data
            LoadOrderData();
            await Task.CompletedTask;
        }

        private void LoadOrderData()
        {
            if (_order == null)
                return;

            ClientName = _order.Client?.ClientName ?? "Unknown Client";
            OrderTypeText = GetOrderTypeText(_order);
            OrderDateText = $"Order Date: {_order.Date:g}";

            if (_order.ShipDate != DateTime.MinValue)
            {
                ShipDateText = $"Ship Date: {_order.ShipDate:g}";
                ShowShipDate = true;
            }
            else
            {
                ShowShipDate = false;
            }

            // Status
            if (_order.Voided)
            {
                StatusText = "Voided";
                StatusColor = Colors.Red;
                ShowStatus = true;
                CanEdit = false;
            }
            else if (_order.Finished)
            {
                StatusText = "Finalized";
                StatusColor = Colors.Green;
                ShowStatus = true;
                CanEdit = false;
            }
            else if (_order.Dexed)
            {
                StatusText = "DEX Sent";
                StatusColor = Colors.Blue;
                ShowStatus = true;
                CanEdit = false;
            }
            else
            {
                ShowStatus = false;
                CanEdit = !_order.Locked();
            }

            // Comments
            if (!string.IsNullOrEmpty(_order.Comments))
            {
                CommentsText = $"Comments: {_order.Comments}";
                ShowComments = true;
            }
            else
            {
                ShowComments = false;
            }

            // Discount visibility
            ShowDiscount = _order.Client?.UseDiscount == true || _order.Client?.UseDiscountPerLine == true || _order.IsDelivery;

            // Load line items
            LineItems.Clear();
            double totalQty = 0;

            foreach (var detail in _order.Details.OrderBy(x => x.Product.Name))
            {
                totalQty += detail.Qty;
                var lineItem = CreateLineItemViewModel(detail);
                LineItems.Add(lineItem);
            }

            // Calculate totals
            var subtotal = _order.OrderTotalCost();
            var discount = _order.DiscountAmount;
            var tax = _order.CalculateTax();
            var total = _order.OrderTotalCost();

            SubtotalText = $"Subtotal: {subtotal.ToCustomString()}";
            DiscountText = $"Discount: {discount.ToCustomString()}";
            TaxText = $"Tax: {tax.ToCustomString()}";
            TotalText = $"Total: {total.ToCustomString()}";
            TotalQtyText = $"Total Qty: {totalQty}";

            // Order summary for catalog view
            LinesText = $"Lines: {_order.Details.Count}";
            QtySoldText = $"Qty Sold: {totalQty}";
            
            // Calculate order and credit amounts
            var orderAmount = _order.Details.Where(x => !x.IsCredit).Sum(x => x.Qty * x.Price);
            var creditAmount = _order.Details.Where(x => x.IsCredit).Sum(x => x.Qty * x.Price);
            OrderAmountText = $"Order: {orderAmount.ToCustomString()}";
            CreditAmountText = $"Credit: {creditAmount.ToCustomString()}";

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

            ShowAddProduct = CanEdit && !_order.Finished && !_order.Voided;
            ShowSendButton = _order.AsPresale;
        }

        private OrderLineItemViewModel CreateLineItemViewModel(OrderDetail detail)
        {
            var qtyText = detail.Product.SoldByWeight && detail.Weight > 0
                ? $"Qty: {detail.Qty} (Weight: {detail.Weight})"
                : $"Qty: {detail.Qty}";

            var priceText = detail.Price > 0
                ? $"Price: {detail.Price.ToCustomString()}"
                : string.Empty;

            var uomText = detail.UnitOfMeasure != null
                ? $"UoM: {detail.UnitOfMeasure.Name}"
                : string.Empty;

            var typeText = string.Empty;
            var typeColor = Colors.Transparent;
            var showType = false;

            if (detail.IsCredit)
            {
                typeText = detail.Damaged ? "Dump" : "Return";
                typeColor = Colors.Orange;
                showType = true;
            }


            return new OrderLineItemViewModel
            {
                Detail = detail,
                ProductName = detail.Product.Name,
                QtyText = qtyText,
                PriceText = priceText,
                UnitOfMeasureText = uomText,
                ShowUnitOfMeasure = !string.IsNullOrEmpty(uomText),
                TypeText = typeText,
                TypeColor = typeColor,
                ShowType = showType
            };
        }

        private string GetOrderTypeText(Order order)
        {
            if (order.OrderType == OrderType.Order)
            {
                if (_asPresale)
                {
                    return order.IsQuote ? "Quote" : "Sales Order";
                }
                return "Sales Invoice";
            }
            else if (order.OrderType == OrderType.Credit)
            {
                return _asPresale ? "Credit Order" : "Credit Invoice";
            }
            else if (order.OrderType == OrderType.Return)
            {
                return _asPresale ? "Return Order" : "Return Invoice";
            }
            else if (order.OrderType == OrderType.Consignment)
            {
                return "Consignment";
            }
            else if (order.OrderType == OrderType.WorkOrder)
            {
                return "Work Order";
            }
            else if (order.OrderType == OrderType.NoService)
            {
                return "No Service";
            }

            return order.OrderType.ToString();
        }
        
        [RelayCommand]
        private async Task LineItemSelectedAsync(OrderLineItemViewModel? item)
        {
            if (item == null || item.Detail == null || _order == null)
                return;
            
            if (Config.UseCatalog)
            {
                // Navigate to AddItemPage to edit/add quantity
                await Shell.Current.GoToAsync($"additem?orderId={_order.OrderId}&orderDetailId={item.Detail.OrderDetailId}");
            }
            else
            {
                // Both configs OFF - navigate to AddItemPage (default behavior)
                await Shell.Current.GoToAsync($"additem?orderId={_order.OrderId}&orderDetailId={item.Detail.OrderDetailId}");
            }
        }

        [RelayCommand]
        private async Task AddProductAsync()
        {
            if (_order == null)
                return;

            // Equivalent to ViewProd_Click in PreviouslyOrderedTemplateActivity
            OrderDetail? lastDetail = null;
            
            if (Config.CatalogQuickAdd)
            {
                lastDetail = _order.Details.OrderByDescending(x => x.OrderDetailId).FirstOrDefault();
            }
            else
            {
                lastDetail = _order.Details.OrderByDescending(x => x.OrderDetailId).FirstOrDefault();
            }

            if (lastDetail != null)
            {
                // Navigate to FullCategoryPage with category and product
                await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&categoryId={lastDetail.Product.CategoryId}&productId={lastDetail.Product.ProductId}");
            }
            else
            {
                // Navigate to categories first
                await ViewCategoriesAsync();
            }
        }

        [RelayCommand]
        private async Task ViewCategoriesAsync()
        {
            if (_order == null)
                return;

            if (Config.UseCatalog)
            {
                // Navigate to categories first
                if (Category.Categories.Count == 1)
                {
                    // Single category - go directly to ProductCatalog
                    var category = Category.Categories.FirstOrDefault();
                    if (category != null)
                    {
                        await Shell.Current.GoToAsync($"productcatalog?orderId={_order.OrderId}&categoryId={category.CategoryId}");
                    }
                }
                else
                {
                    // Multiple categories - show category selection (FullCategoryPage in category mode)
                    // FullCategoryPage will redirect to ProductCatalog when a category is selected
                    await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&clientId={_order.Client.ClientId}");
                }
            }
            else
            {
                // Both configs OFF - navigate to FullCategoryPage (default behavior)
                // FullCategoryPage will navigate to AddItemPage when product is selected
                if (Category.Categories.Count == 1)
                {
                    // Single category - go directly to product list
                    var category = Category.Categories.FirstOrDefault();
                    if (category != null)
                    {
                        await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&categoryId={category.CategoryId}");
                    }
                }
                else
                {
                    // Multiple categories - show category selection (FullCategoryPage in category mode)
                    await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&clientId={_order.Client.ClientId}");
                }
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            if (_order == null)
                return;

            // Equivalent to Search_Click - show search dialog
            var searchOptions = new[] { "Search in Product List", "Search in Current Transaction" };
            var choice = await _dialogService.ShowActionSheetAsync("Search", "Cancel", null, searchOptions);
            
            if (choice == null || choice == "Cancel")
                return;

            var searchTerm = await _dialogService.ShowPromptAsync("Enter Product Name", "Search", "OK", "Cancel", "Product name, UPC, SKU, or code");
            
            if (string.IsNullOrWhiteSpace(searchTerm))
                return;

            searchTerm = searchTerm.ToLowerInvariant().Trim();

            if (choice == "Search in Current Transaction")
            {
                // Search in current order details
                var matchingLine = LineItems.FirstOrDefault(x => 
                    x.ProductName.ToLowerInvariant().IndexOf(searchTerm) != -1);
                
                if (matchingLine != null)
                {
                    // TODO: Scroll to matching line
                    await _dialogService.ShowAlertAsync($"Found: {matchingLine.ProductName}", "Search Result");
                }
                else
                {
                    await _dialogService.ShowAlertAsync("No products found in current transaction.", "Search");
                }
            }
            else
            {
                // Search in product list - equivalent to DoSearchResult
                var products = Product.GetProductListForOrder(_order, false, 0, true).ToList();
                
                var matchingProducts = products.Where(x => (
                    x.Name.ToLowerInvariant().IndexOf(searchTerm) != -1 ||
                    x.Upc.ToLowerInvariant().Contains(searchTerm) ||
                    x.Sku.ToLowerInvariant().Contains(searchTerm) ||
                    x.Description.ToLowerInvariant().Contains(searchTerm) ||
                    x.Code.ToLowerInvariant().Contains(searchTerm)
                ) && (x.CategoryId != 0)).ToList();

                if (Config.ButlerCustomization && _order.Client.PriceLevel > 0)
                {
                    var productPrices = ProductPrice.Pricelist.Where(x => x.PriceLevelId == _order.Client.PriceLevel).Select(x => x.ProductId).Distinct().ToList();
                    matchingProducts = matchingProducts.Where(x => productPrices.Contains(x.ProductId)).ToList();
                }

                if (matchingProducts.Count == 0)
                {
                    await _dialogService.ShowAlertAsync("No products found.", "Search");
                }
                else if (matchingProducts.Count == 1)
                {
                    var product = matchingProducts.First();
                    
                    // Check inventory
                    if (product.GetInventory(_order.AsPresale) <= 0)
                    {
                        await _dialogService.ShowAlertAsync($"Not enough inventory of {product.Name}", "Alert");
                        return;
                    }

                    // Check availability
                    if (!string.IsNullOrEmpty(product.NonVisibleExtraFieldsAsString))
                    {
                        var available = DataAccess.GetSingleUDF("AvailableIn", product.NonVisibleExtraFieldsAsString);
                        if (!string.IsNullOrEmpty(available))
                        {
                            if (available.ToLower() == "none" || !available.ToLower().Contains("order"))
                            {
                                await _dialogService.ShowAlertAsync("Product unavailable.", "Warning");
                                return;
                            }
                        }
                    }

                    // Navigate to AddItemPage or directly add
                    // For now, navigate to FullCategoryPage with product search
                    await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&productSearch={searchTerm}&comingFromSearch=yes");
                }
                else
                {
                    // Multiple products - navigate to product list with search
                    await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&productSearch={searchTerm}&comingFromSearch=yes");
                }
            }
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (_order == null)
                return;

            var canNavigate = await FinalizeOrderAsync();
            if (canNavigate)
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        public async Task<bool> FinalizeOrderAsync()
        {
            if (_order == null)
                return true;

            // Check if order is voided
            if (_order.Voided)
            {
                return true; // Allow navigation
            }

            // Check if order is empty
            bool isEmpty = _order.Details.Count == 0 || 
                (_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem);

            if (isEmpty)
            {
                // Remove if empty order
                if (_order.AsPresale)
                {
                    UpdateRoute(false);

                    if ((_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem) ||
                        (_order.Details.Count == 0 && Order.Orders.Count(x => x.BatchId == _order.BatchId) == 1))
                    {
                        var batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
                        if (batch != null)
                        {
                            Logger.CreateLog($"Batch with id={batch.Id} DELETED (1 order without details)");
                            batch.Delete();
                        }
                    }
                }

                if (string.IsNullOrEmpty(_order.PrintedOrderId) && !_order.IsDelivery)
                {
                    Logger.CreateLog($"Order with id={_order.OrderId} DELETED (no details)");
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
                        Logger.CreateLog($"Order with id={_order.OrderId} VOIDED from template");

                        if (_order.IsDelivery && string.IsNullOrEmpty(_order.PrintedOrderId))
                            _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);

                        _order.Finished = true;
                        _order.Void();
                        _order.Save();
                        return true; // Allow navigation
                    }
                    return false; // Don't navigate
                }
            }
            else
            {
                // Make sure that all the weighted lines have value
                if (!_order.AsPresale)
                {
                    if (_order.Details.Any(x => x.Product.SoldByWeight && x.Weight == 0) && !Config.MustSetWeightInDelivery)
                    {
                        var result = await _dialogService.ShowConfirmAsync(
                            "Some weight items don't have weight. Do you want to delete them?",
                            "Warning",
                            "Yes",
                            "No");
                        if (result)
                        {
                            // Delete 0 weight items
                            var toDelete = _order.Details.Where(x => x.Product.SoldByWeight && x.Weight == 0).ToList();
                            foreach (var detail in toDelete)
                            {
                                _order.Details.Remove(detail);
                            }
                            _order.Save();
                            // Recursively call FinalizeOrder
                            return await FinalizeOrderAsync();
                        }
                        return false; // Don't navigate
                    }
                }

                // Check mandatory image
                if (Config.ImageInOrderMandatory && _order.ImageList.Count == 0)
                {
                    await _dialogService.ShowAlertAsync("Order image is mandatory.", "Alert");
                    return false; // Don't navigate
                }

                // Check mandatory PO
                if ((Config.POIsMandatory || _order.Client.POIsMandatory) && string.IsNullOrEmpty(_order.PONumber) && _order.Client.POIsMandatory)
                {
                    await _dialogService.ShowAlertAsync("You need to enter PO number.", "Alert");
                    return false; // Don't navigate
                }

                // Check Bill number
                if (_order.OrderType == OrderType.Bill && string.IsNullOrEmpty(_order.PONumber) && Config.BillNumRequired)
                {
                    await _dialogService.ShowAlertAsync("You need to enter Bill number.", "Alert");
                    return false; // Don't navigate
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

                // Check case in/out requirements
                if (Config.MustEnterCaseInOut && (_order.OrderType == OrderType.Order || _order.OrderType == OrderType.Credit) && !_order.AsPresale)
                {
                    // TODO: Implement EnterCasesInOut
                    await _dialogService.ShowAlertAsync("Case in/out functionality is not yet fully implemented.", "Info");
                    return false; // Don't navigate
                }

                // Add to session
                if (Session.session != null)
                    Session.session.AddDetailFromOrder(_order);

                // Set end date
                if (_order.EndDate == DateTime.MinValue)
                {
                    _order.EndDate = DateTime.Now;
                    _order.Save();
                }

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

                // Check lot mandatory
                if (Config.LotIsMandatoryBeforeFinalize && _order.Details.Any(x => string.IsNullOrEmpty(x.Lot)))
                {
                    await _dialogService.ShowAlertAsync("Lot is mandatory.", "Alert");
                    return false; // Don't navigate
                }

                // Check for alerts
                if (_order.AsPresale && Config.AlertOrderWasNotSent)
                {
                    var result = await _dialogService.ShowConfirmAsync(
                        "This order was not sent. If you continue it will be saved in the device to be sent later. Are you sure you want to continue?",
                        "Alert",
                        "Yes",
                        "No");
                    if (!result)
                        return false; // Don't navigate
                }

                // Check for discounts
                var shipdate = _order.ShipDate != DateTime.MinValue ? _order.ShipDate : DateTime.Now;
                if (_order.AsPresale && !Config.AlertOrderWasNotSent && OrderDiscount.HasDiscounts && _order.DiscountAmount == 0 && 
                    _order.Details.Any(x => OrderDiscount.ProductHasDiscount(x.Product, x.Qty, _order, shipdate, x.UnitOfMeasure, x.IsFreeItem)))
                {
                    var result = await _dialogService.ShowConfirmAsync(
                        "This client has discounts available to apply. Are you sure you want to continue?",
                        "Alert",
                        "Yes",
                        "No");
                    if (!result)
                        return false; // Don't navigate
                }

                // Check for suggested categories
                if (_order.AsPresale && SuggestedClientCategory.List.Count > 0)
                {
                    var suggestedForThisClient = SuggestedClientCategory.List.FirstOrDefault(x => 
                        x.SuggestedClientCategoryClients.Any(y => y.ClientId == _order.Client.ClientId));

                    if (suggestedForThisClient != null)
                    {
                        var product_ = Product.GetProductListForOrder(_order, false, 0);
                        bool containedInOrder = true;

                        foreach (var p in suggestedForThisClient.SuggestedClientCategoryProducts)
                        {
                            if (!product_.Any(x => x.ProductId == p.ProductId))
                                continue;

                            if (!_order.Details.Any(x => x.Product.ProductId == p.ProductId))
                            {
                                containedInOrder = false;
                                break;
                            }
                        }

                        if (!containedInOrder)
                        {
                            var categoryName = string.IsNullOrEmpty(Config.ProductCategoryNameIdentifier) 
                                ? "suggested" 
                                : Config.ProductCategoryNameIdentifier;
                            var result = await _dialogService.ShowConfirmAsync(
                                $"Continue without {categoryName} items?",
                                "Alert",
                                "Yes",
                                "No");
                            if (!result)
                            {
                                // TODO: Navigate to suggested items
                                await _dialogService.ShowAlertAsync("Suggested items functionality is not yet fully implemented.", "Info");
                                return false; // Don't navigate
                            }
                        }
                    }
                }

                return true; // Allow navigation
            }
        }

        [RelayCommand]
        private async Task ShowMenuAsync()
        {
            if (_order == null)
                return;

            var options = BuildMenuOptions();
            if (options.Count == 0)
                return;

            var choice = await _dialogService.ShowActionSheetAsync("Menu", "Cancel", null, options.Select(o => o.Title).ToArray());
            if (string.IsNullOrWhiteSpace(choice))
                return;

            var option = options.FirstOrDefault(o => o.Title == choice);
            if (option?.Action != null)
            {
                await option.Action();
            }
        }

        private List<MenuOption> BuildMenuOptions()
        {
            var options = new List<MenuOption>();

            if (_order == null)
                return options;

            var finalized = _order.Finished;
            var allowDiscount = _order.Client.UseDiscount;

            if (_asPresale)
            {
                // Presale menu items
                if (allowDiscount && !_order.Locked())
                {
                    options.Add(new MenuOption("Add Discount", async () =>
                    {
                        await _dialogService.ShowAlertAsync("Add Discount functionality is not yet fully implemented.", "Info");
                    }));
                }

                if (Config.SetPO && (_order.OrderType != OrderType.Order || Config.POIsMandatory))
                {
                    options.Add(new MenuOption("Set PO", async () =>
                    {
                        var po = await _dialogService.ShowPromptAsync("Set PO", "Enter PO Number:", initialValue: _order.PONumber ?? string.Empty);
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
                        await _dialogService.ShowAlertAsync("Print functionality is not yet fully implemented.", "Info");
                    }));
                }

                options.Add(new MenuOption("Set Ship Date", async () =>
                {
                    var currentShipDate = _order.ShipDate.Year == 1 ? DateTime.Now : _order.ShipDate;
                    var selectedDate = await _dialogService.ShowDatePickerAsync("Set Ship Date", currentShipDate, DateTime.Now, null);
                    if (selectedDate.HasValue)
                    {
                        _order.ShipDate = selectedDate.Value;
                        _order.Save();
                        ShipDateText = _order.ShipDate.ToShortDateString();
                        ShowShipDate = true;
                    }
                }));

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

                    options.Add(new MenuOption("Share PDF", async () =>
                    {
                        await _dialogService.ShowAlertAsync("Share PDF functionality is not yet fully implemented.", "Info");
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
                        var po = await _dialogService.ShowPromptAsync("Set PO", "Enter PO Number:", initialValue: _order.PONumber ?? string.Empty);
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
                        await _dialogService.ShowAlertAsync("Add Discount functionality is not yet fully implemented.", "Info");
                    }));
                }

                if (!Config.LockOrderAfterPrinted)
                {
                    var isSplitClient = _order.Client.SplitInvoices.Count > 0;
                    if (!isSplitClient || _order.Finished)
                    {
                        options.Add(new MenuOption("Print", async () =>
                        {
                            await _dialogService.ShowAlertAsync("Print functionality is not yet fully implemented.", "Info");
                        }));
                    }
                }

                if (!(_order.Client.SplitInvoices.Count > 0))
                {
                    options.Add(new MenuOption("Send by Email", async () =>
                    {
                        await SendByEmailAsync();
                    }));

                    options.Add(new MenuOption("Share PDF", async () =>
                    {
                        await _dialogService.ShowAlertAsync("Share PDF functionality is not yet fully implemented.", "Info");
                    }));
                }
            }

            // Common menu items
            options.Add(new MenuOption("Add Comments", async () =>
            {
                var comments = await _dialogService.ShowPromptAsync("Add Comments", "Enter comments:", initialValue: _order.Comments ?? string.Empty);
                if (comments != null)
                {
                    _order.Comments = comments;
                    _order.Save();
                    await _dialogService.ShowAlertAsync("Comments saved.", "Success");
                }
            }));

            options.Add(new MenuOption("Advanced Options", ShowAdvancedOptionsAsync));

            return options;
        }

        private async Task ShowAdvancedOptionsAsync()
        {
            await _advancedOptionsService.ShowAdvancedOptionsAsync();
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

            // Validate order minimum
            bool valid = _order.ValidateOrderMinimum();
            if (!valid)
                return;

            // Validate presale requirements
            if (_order.AsPresale)
            {
                var totalWeight = _order.TotalWeight;
                if (Config.MinimumWeight > 0 && totalWeight < Config.MinimumWeight)
                {
                    await _dialogService.ShowAlertAsync(
                        $"Order minimum total weight is {Config.MinimumWeight}, current weight is {totalWeight}",
                        "Alert");
                    return;
                }

                var orderCost = _order.OrderTotalCost();
                if (!Config.Simone && Config.MinimumAmount > 0 && orderCost < Config.MinimumAmount)
                {
                    await _dialogService.ShowAlertAsync(
                        $"Order minimum total amount is {Config.MinimumAmount.ToCustomString()}, current amount is {orderCost.ToCustomString()}",
                        "Alert");
                    return;
                }

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

                // Check for ShipVia
                if (Config.ShipViaMandatory)
                {
                    var shipVia = DataAccess.GetSingleUDF("ShipVia", _order.ExtraFields);
                    if (string.IsNullOrEmpty(shipVia))
                    {
                        await _dialogService.ShowAlertAsync("Must add Ship Via.", "Alert");
                        return;
                    }
                }

                // Check for Disol Survey
                if (Config.UseDisolSurvey && _order.OrderType == OrderType.Order && !_order.HasDisolSurvey)
                {
                    string survey = DataAccess.GetSingleUDF("Survey", _order.Client.ExtraPropertiesAsString);
                    bool mustFillSurvey = survey == "1";
                    if (mustFillSurvey)
                    {
                        var result = await _dialogService.ShowConfirmAsync(
                            "You must complete a survey before continuing. Do you want to do the survey now?",
                            "Alert",
                            "Yes",
                            "No");
                        if (result)
                        {
                            // TODO: Navigate to survey
                            await _dialogService.ShowAlertAsync("Survey functionality is not yet fully implemented.", "Info");
                            return;
                        }
                        return;
                    }
                }

                // Check for mandatory images
                if (Config.CaptureImages && Config.ImageInOrderMandatory && _order.ImageList.Count <= 0)
                {
                    await _dialogService.ShowAlertAsync("Order image is mandatory to send presale order.", "Warning");
                    return;
                }

                // Check for discounts
                var shipdate = _order.ShipDate != DateTime.MinValue ? _order.ShipDate : DateTime.Now;
                if (OrderDiscount.HasDiscounts && _order.DiscountAmount == 0 && 
                    _order.Details.Any(x => OrderDiscount.ProductHasDiscount(x.Product, x.Qty, _order, shipdate, x.UnitOfMeasure, x.IsFreeItem)))
                {
                    var result = await _dialogService.ShowConfirmAsync(
                        "This client has discounts available to apply. Are you sure you want to continue sending it?",
                        "Alert",
                        "Yes",
                        "No");
                    if (!result)
                        return;
                }

                // Check if ship date is locked
                if (Config.CheckIfShipdateLocked)
                {
                    var lockedDates = new List<DateTime>();
                    if (!DataAccess.CheckIfShipdateIsValid(new List<DateTime>() { _order.ShipDate }, ref lockedDates))
                    {
                        var sb = string.Empty;
                        foreach (var l in lockedDates)
                            sb += '\n' + l.Date.ToShortDateString();
                        await _dialogService.ShowAlertAsync("The selected date is currently locked. Please select a different shipdate", "Alert");
                        return;
                    }
                }

                // Check for suggested categories
                if (SuggestedClientCategory.List.Count > 0)
                {
                    var suggestedForThisClient = SuggestedClientCategory.List.FirstOrDefault(x => 
                        x.SuggestedClientCategoryClients.Any(y => y.ClientId == _order.Client.ClientId));

                    if (suggestedForThisClient != null)
                    {
                        var product_ = Product.GetProductListForOrder(_order, false, 0);
                        bool containedInOrder = true;

                        foreach (var p in suggestedForThisClient.SuggestedClientCategoryProducts)
                        {
                            if (!product_.Any(x => x.ProductId == p.ProductId))
                                continue;

                            if (!_order.Details.Any(x => x.Product.ProductId == p.ProductId))
                            {
                                containedInOrder = false;
                                break;
                            }
                        }

                        if (!containedInOrder)
                        {
                            var categoryName = string.IsNullOrEmpty(Config.ProductCategoryNameIdentifier) 
                                ? "suggested" 
                                : Config.ProductCategoryNameIdentifier;
                            var result = await _dialogService.ShowConfirmAsync(
                                $"Continue sending without {categoryName} items?",
                                "Alert",
                                "Yes",
                                "No");
                            if (!result)
                            {
                                // TODO: Navigate to suggested items
                                await _dialogService.ShowAlertAsync("Suggested items functionality is not yet fully implemented.", "Info");
                                return;
                            }
                        }
                    }
                }
            }

            // Check if discounts need recalculation
            if (OrderDiscount.HasDiscounts && _order.NeedToCalculate && !Config.CalculateOffersAutomatically)
            {
                await _dialogService.ShowAlertAsync("You need to recalculate the offers before sending the order.", "Alert");
                return;
            }

            // Show confirmation dialog
            var message = _order.IsQuote ? "Continue sending quote?" : "Continue sending order?";
            var confirm = await _dialogService.ShowConfirmAsync(message, "Warning", "Yes", "No");
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
                    ShipDateText = _order.ShipDate.ToShortDateString();
                    ShowShipDate = true;
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

            // Validate order minimum again before sending
            valid = _order.ValidateOrderMinimum();
            if (!valid)
                return;

            // Send the order
            await SendItAsync();
        }

        private async Task SendItAsync()
        {
            if (_order == null)
                return;

            try
            {
                await _dialogService.ShowLoadingAsync(_order.IsQuote ? "Sending quote..." : "Sending order...");

                // Recalculate discounts if needed
                if (OrderDiscount.HasDiscounts)
                {
                    _order.RecalculateDiscounts();
                    if (!Config.CalculateOffersAutomatically)
                        _order.VerifyDiscount();
                }

                // Check inventory if needed
                if (Config.CheckAvailableBeforeSending && !Config.CanGoBelow0 && Config.CheckInventoryInPreSale)
                {
                    DataAccessEx.GetInventoryInBackground(true);

                    List<Order> validOrders = new List<Order>();
                    if (_order.AsPresale && _order.OrderType == OrderType.Order)
                        validOrders.Add(_order);

                    bool canSend = true;
                    foreach (var order in validOrders)
                    {
                        foreach (var detail in order.Details)
                        {
                            var factor = detail.IsCredit ? 1 : -1;
                            var currentOH = detail.Product.GetInventory(order.AsPresale, false);
                            order.UpdateInventory(detail, factor);

                            float baseQty = detail.Qty;
                            if (detail.UnitOfMeasure != null)
                                baseQty *= detail.UnitOfMeasure.Conversion;

                            if (currentOH - baseQty < 0)
                            {
                                canSend = false;
                                break;
                            }
                        }
                        if (!canSend)
                            break;
                    }

                    if (!canSend)
                    {
                        await _dialogService.HideLoadingAsync();
                        await _dialogService.ShowAlertAsync(
                            "You have one or more products that are currently out of stock. Please fix the order and try again.",
                            "Alert");
                        return;
                    }
                }

                // Set end date if not set
                if (_order.EndDate == DateTime.MinValue)
                {
                    _order.EndDate = DateTime.Now;
                }

                // Generate presale number if needed
                if (_order.AsPresale && Config.GeneratePresaleNumber && string.IsNullOrEmpty(_order.PrintedOrderId))
                {
                    _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);
                }

                // Update route if presale
                if (_order.AsPresale)
                {
                    UpdateRoute(true);
                }

                _order.Save();

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
                DataAccess.SendTheOrders(new Batch[] { batch });

                await _dialogService.HideLoadingAsync();

                var successMessage = _order.IsQuote 
                    ? "Quote sent successfully." 
                    : "Order sent successfully.";
                await _dialogService.ShowAlertAsync(successMessage, "Success");

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
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog(ex);
                var errorMessage = _order.IsQuote 
                    ? "Error sending quote." 
                    : "Error sending order.";
                await _dialogService.ShowAlertAsync(errorMessage, "Alert");
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

        private record MenuOption(string Title, Func<Task> Action);

        private async Task SendByEmailAsync()
        {
            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("No order to send.", "Alert", "OK");
                return;
            }

            try
            {
                // Use PdfHelper to send order by email (matches Xamarin template activities)
                await PdfHelper.SendOrderByEmail(_order);
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
            }
        }
    }

    public partial class OrderLineItemViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _productName = string.Empty;

        [ObservableProperty]
        private string _qtyText = string.Empty;

        [ObservableProperty]
        private string _priceText = string.Empty;

        [ObservableProperty]
        private string _amountText = string.Empty;

        [ObservableProperty]
        private string _unitOfMeasureText = string.Empty;

        [ObservableProperty]
        private bool _showUnitOfMeasure;

        [ObservableProperty]
        private string _typeText = string.Empty;

        [ObservableProperty]
        private Color _typeColor;

        [ObservableProperty]
        private bool _showType;

        public OrderDetail Detail { get; set; } = null!;
    }
}

