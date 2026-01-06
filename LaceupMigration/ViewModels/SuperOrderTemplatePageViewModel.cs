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
    public partial class SuperOrderTemplatePageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        private Order? _order;
        private Order? _credit;
        private bool _asPresale;
        private bool _initialized;
        private int _lastDetailCount = 0;
        private int? _lastDetailId = null;

        public ObservableCollection<SuperOrderLineItemViewModel> LineItems { get; } = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _shipDateText = string.Empty;

        [ObservableProperty]
        private bool _showShipDate;

        [ObservableProperty]
        private string _linesText = "Lines: 0";

        [ObservableProperty]
        private string _subtotalText = "Subtotal: $0.00";

        [ObservableProperty]
        private string _taxText = "Tax: $0.00";

        [ObservableProperty]
        private string _discountText = "Discount: $0.00";

        [ObservableProperty]
        private string _totalText = "Total: $0.00";

        [ObservableProperty]
        private bool _showTotals = true;

        [ObservableProperty]
        private bool _showTax = true;

        [ObservableProperty]
        private bool _showDiscount = true;

        [ObservableProperty]
        private bool _canEdit = true;

        [ObservableProperty]
        private bool _showAddProduct = true;

        [ObservableProperty]
        private bool _showViewCategories = true;

        [ObservableProperty]
        private bool _showSearch = true;

        [ObservableProperty]
        private string _doneButtonText = "Done";

        public SuperOrderTemplatePageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
            ShowTotals = !Config.HidePriceInTransaction;
        }

        public async Task InitializeAsync(int orderId, int creditId, bool asPresale)
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

            if (creditId > 0)
            {
                _credit = Order.Orders.FirstOrDefault(x => x.OrderId == creditId);
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

            // Equivalent to OnResume - Update UI state
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
            if (_order == null)
                return;

            ClientName = _order.Client?.ClientName ?? "Unknown Client";

            if (_order.ShipDate != DateTime.MinValue)
            {
                ShipDateText = $"Ship Date: {_order.ShipDate:g}";
                ShowShipDate = true;
            }
            else
            {
                ShowShipDate = false;
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

            if (_asPresale)
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
            if (_order == null)
                return;

            var allDetails = _order.Details.ToList();
            if (_credit != null)
            {
                allDetails.AddRange(_credit.Details);
            }

            LinesText = $"Lines: {allDetails.Count}";

            var subtotal = _order.CalculateItemCost();
            if (_credit != null)
                subtotal += _credit.CalculateItemCost();
            SubtotalText = $"Subtotal: {subtotal.ToCustomString()}";

            var tax = _order.CalculateTax();
            if (_credit != null)
                tax += _credit.CalculateTax();
            TaxText = $"Tax: {tax.ToCustomString()}";
            ShowTax = _order.TaxRate > 0;

            var discount = _order.CalculateDiscount();
            if (_credit != null)
                discount += _credit.CalculateDiscount();
            DiscountText = $"Discount: {discount.ToCustomString()}";
            ShowDiscount = Config.AllowDiscount || _order.Client?.UseDiscount == true || _order.Client?.UseDiscountPerLine == true;

            var total = _order.OrderTotalCost();
            if (_credit != null)
                total += _credit.OrderTotalCost();
            TotalText = $"Total: {total.ToCustomString()}";

            // Load line items
            LineItems.Clear();
            var sortedDetails = SortDetails.SortedDetails(allDetails).ToList();

            foreach (var detail in sortedDetails)
            {
                var lineItem = CreateLineItemViewModel(detail);
                LineItems.Add(lineItem);
            }
        }

        private SuperOrderLineItemViewModel CreateLineItemViewModel(OrderDetail detail)
        {
            var qtyText = detail.Product.SoldByWeight && detail.Weight > 0
                ? $"Qty: {detail.Qty} (Weight: {detail.Weight})"
                : $"Qty: {detail.Qty}";

            var priceText = detail.Price > 0
                ? $"Price: {detail.Price.ToCustomString()}"
                : string.Empty;

            var typeText = string.Empty;
            var typeColor = Microsoft.Maui.Graphics.Colors.Transparent;
            var showType = false;

            if (detail.IsCredit)
            {
                typeText = detail.Damaged ? "Dump" : "Return";
                typeColor = Microsoft.Maui.Graphics.Colors.Orange;
                showType = true;
            }
            
            return new SuperOrderLineItemViewModel
            {
                Detail = detail,
                ProductName = detail.Product.Name,
                QtyText = qtyText,
                PriceText = priceText,
                TypeText = typeText,
                TypeColor = typeColor,
                ShowType = showType
            };
        }

        [RelayCommand]
        private async Task LineItemSelectedAsync(SuperOrderLineItemViewModel? item)
        {
            if (item == null || item.Detail == null)
                return;

            // TODO: Navigate to line item detail/edit page
            await _dialogService.ShowAlertAsync($"Selected: {item.ProductName}", "Info");
        }

        [RelayCommand]
        private async Task AddProductAsync()
        {
            if (_order == null)
                return;

            // Similar to PreviouslyOrderedTemplateActivity ViewProd_Click
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
                await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&categoryId={lastDetail.Product.CategoryId}&productId={lastDetail.Product.ProductId}");
            }
            else
            {
                await ViewCategoriesAsync();
            }
        }

        [RelayCommand]
        private async Task ViewCategoriesAsync()
        {
            if (_order == null)
                return;

            // Similar to PreviouslyOrderedTemplateActivity ShowCategoryActivity
            if (Category.Categories.Count == 1)
            {
                var category = Category.Categories.FirstOrDefault();
                if (category != null)
                {
                    await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&categoryId={category.CategoryId}");
                }
            }
            else
            {
                await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&clientId={_order.Client.ClientId}");
            }
        }

        [RelayCommand]
        private async Task SearchAsync()
        {
            if (_order == null)
                return;

            // Similar to PreviouslyOrderedTemplateActivity Search_Click and DoSearchResult
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
                var matchingLine = LineItems.FirstOrDefault(x => 
                    x.ProductName.ToLowerInvariant().IndexOf(searchTerm) != -1);
                
                if (matchingLine != null)
                {
                    await _dialogService.ShowAlertAsync($"Found: {matchingLine.ProductName}", "Search Result");
                }
                else
                {
                    await _dialogService.ShowAlertAsync("No products found in current transaction.", "Search");
                }
            }
            else
            {
                // Search in product list
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
                    
                    if (product.GetInventory(_order.AsPresale) <= 0)
                    {
                        await _dialogService.ShowAlertAsync($"Not enough inventory of {product.Name}", "Alert");
                        return;
                    }

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

                    await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&productSearch={searchTerm}&comingFromSearch=yes");
                }
                else
                {
                    await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&productSearch={searchTerm}&comingFromSearch=yes");
                }
            }
        }

        [RelayCommand]
        private async Task DoneAsync()
        {
            if (_order == null)
                return;

            _order.Modified = true;
            _order.Save();

            if (_credit != null)
            {
                _credit.Modified = true;
                _credit.Save();
            }

            if (_asPresale)
            {
                await SendOrderAsync();
            }
            else
            {
                var canNavigate = await FinalizeOrderAsync();
                if (canNavigate)
                {
                    await Shell.Current.GoToAsync("..");
                }
            }
        }

        public async Task<bool> FinalizeOrderAsync()
        {
            if (_order == null)
                return true;

            // Similar to OrderDetailsPageViewModel but handle both order and credit
            // Check if order is voided
            if (_order.Voided)
            {
                return true; // Allow navigation
            }

            // Check if order is empty (check both order and credit)
            bool isEmpty = (_order.Details.Count == 0 || 
                (_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem)) &&
                (_credit == null || _credit.Details.Count == 0);

            if (isEmpty)
            {
                // Similar logic to OrderDetailsPageViewModel
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
                    if (_credit != null)
                        _credit.Delete();
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
                // Similar validations to OrderDetailsPageViewModel
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
        }

        private async Task SendOrderAsync()
        {
            if (_order == null)
                return;

            // Similar validation to OrderDetailsPageViewModel but handle both order and credit
            bool orderIsEmpty = _order.Details.Count == 0 || 
                (_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem);
            bool creditIsEmpty = _credit == null || _credit.Details.Count == 0 || 
                (_credit.Details.Count == 1 && _credit.Details[0].Product.ProductId == Config.DefaultItem);
            
            if (orderIsEmpty && creditIsEmpty)
            {
                await _dialogService.ShowAlertAsync("You can't send an empty order", "Alert");
                return;
            }

            // Use the same validation logic as OrderDetailsPageViewModel
            // For simplicity, we'll reuse the same validation pattern
            bool valid = _order.ValidateOrderMinimum();
            if (!valid)
                return;

            // Validate presale requirements (similar to OrderDetailsPageViewModel)
            if (_order.AsPresale)
            {
                if (string.IsNullOrEmpty(_order.Comments) && Config.PresaleCommMandatory)
                {
                    await _dialogService.ShowAlertAsync("Please provide a comment.", "Alert");
                    return;
                }

                if (Config.SignatureNameRequired && string.IsNullOrEmpty(_order.SignatureName))
                {
                    await _dialogService.ShowAlertAsync("Signature name is required.", "Alert");
                    return;
                }

                if (Config.CaptureImages && Config.ImageInOrderMandatory && _order.ImageList.Count <= 0)
                {
                    await _dialogService.ShowAlertAsync("Order image is mandatory to send presale order.", "Warning");
                    return;
                }

                if (Config.CheckIfShipdateLocked)
                {
                    var lockedDates = new List<DateTime>();
                    if (!DataAccess.CheckIfShipdateIsValid(new List<DateTime>() { _order.ShipDate }, ref lockedDates))
                    {
                        await _dialogService.ShowAlertAsync("The selected date is currently locked. Please select a different shipdate", "Alert");
                        return;
                    }
                }
            }

            // Show confirmation
            var confirm = await _dialogService.ShowConfirmAsync("Continue sending order?", "Warning", "Yes", "No");
            if (!confirm)
                return;

            // Handle ship date
            if (Config.PresaleShipDate && _order.ShipDate.Year == 1)
            {
                await _dialogService.ShowAlertAsync("Please set ship date before sending.", "Alert");
                return;
            }
            else if (Config.ShipDateIsMandatory && _order.ShipDate.Year == 1)
            {
                await _dialogService.ShowAlertAsync("Please select ship date.", "Alert");
                return;
            }

            await SendItAsync();
        }

        private async Task SendItAsync()
        {
            if (_order == null)
                return;

            try
            {
                await _dialogService.ShowLoadingAsync("Sending order...");

                // Save both order and credit
                _order.Modified = true;
                _order.Save();

                if (_credit != null)
                {
                    _credit.Modified = true;
                    _credit.Save();
                }

                // Recalculate discounts
                if (OrderDiscount.HasDiscounts)
                {
                    _order.RecalculateDiscounts();
                    if (!Config.CalculateOffersAutomatically)
                        _order.VerifyDiscount();
                }

                // Set end date
                if (_order.EndDate == DateTime.MinValue)
                    _order.EndDate = DateTime.Now;

                // Generate presale number
                if (_order.AsPresale && Config.GeneratePresaleNumber && string.IsNullOrEmpty(_order.PrintedOrderId))
                    _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);

                // Update route
                if (_order.AsPresale)
                    UpdateRoute(true);

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
                await _dialogService.ShowAlertAsync("Order sent successfully.", "Success");

                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error sending order.", "Alert");
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

            // Xamarin PreviouslyOrderedTemplateActivity logic:
            // If !AsPresale && (Finished || Voided), only show Print option
            bool isReadOnly = !_order.AsPresale && (_order.Finished || _order.Voided);
            
            if (isReadOnly)
            {
                // Only allow Print when read-only
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
                        await _dialogService.ShowAlertAsync("Send by Email functionality is not yet fully implemented.", "Info");
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
                        await _dialogService.ShowAlertAsync("Send by Email functionality is not yet fully implemented.", "Info");
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

        private async Task SendByEmailAsync()
        {
            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("No order to send.", "Alert", "OK");
                return;
            }

            try
            {
                // Use PdfHelper to send order by email (matches Xamarin SuperOrderTemplateActivity)
                // If there's a credit order, send both
                if (_credit != null)
                {
                    await PdfHelper.SendOrdersByEmail(new List<Order> { _order, _credit });
                }
                else
                {
                    await PdfHelper.SendOrderByEmail(_order);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog(ex);
                await _dialogService.ShowAlertAsync("Error occurred sending email.", "Alert", "OK");
            }
        }
    }

    public partial class SuperOrderLineItemViewModel : ObservableObject
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
        private string _typeText = string.Empty;

        [ObservableProperty]
        private Microsoft.Maui.Graphics.Color _typeColor;

        [ObservableProperty]
        private bool _showType;

        public OrderDetail Detail { get; set; } = null!;
    }
}

