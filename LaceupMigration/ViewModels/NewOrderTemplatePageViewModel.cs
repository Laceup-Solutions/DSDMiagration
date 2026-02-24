using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Helpers;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using LaceupMigration;

namespace LaceupMigration.ViewModels
{
    /// <summary>
    /// ViewModel for the Order Template screen (same logic as Xamarin NewOrderTemplateActivity).
    /// Uses template lines (StandartTemplateLine / GroupedTemplateLine) and same menus/totals as PreviouslyOrderedTemplatePage.
    /// </summary>
    public partial class NewOrderTemplatePageViewModel : ObservableObject
    {
        public readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        protected Order? _order;
        private bool _asPresale;
        private bool _initialized;
        private int? _pendingOrderId;
        private bool _pendingAsPresale;
        private List<TemplateLine> _lines = new();
        private SortDetails.SortCriteria _sortCriteria = SortDetails.SortCriteria.ProductName;

        /// <summary>True when this is the Credit template (NewCreditTemplatePage). Used for Save/back and for opening template details.</summary>
        protected virtual bool FromCreditTemplate => false;

        /// <summary>Route name for navigation state (newordertemplate / newcredittemplate). Override in credit.</summary>
        protected virtual string GetTemplateRouteName() => "newordertemplate";

        public ObservableCollection<TemplateLineItemViewModel> TemplateLines { get; } = new();

        #region Header / Totals (same as PreviouslyOrderedTemplatePage)

        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _companyText = string.Empty;
        [ObservableProperty] private bool _showCompany;
        [ObservableProperty] private string _orderTypeText = string.Empty;
        [ObservableProperty] private string _linesText = "Lines: 0";
        [ObservableProperty] private string _qtySoldText = "Qty Sold: 0";
        [ObservableProperty] private string _orderAmountText = "Order: $0.00";
        [ObservableProperty] private string _creditAmountText = "Credit: $0.00";
        [ObservableProperty] private string _subtotalText = "Subtotal: $0.00";
        [ObservableProperty] private string _taxText = "Tax: $0.00";
        [ObservableProperty] private string _discountText = "Discount: $0.00";
        [ObservableProperty] private string _totalText = "Total: $0.00";
        [ObservableProperty] private string _termsText = "Terms: ";
        [ObservableProperty] private bool _termsVisible = true;
        [ObservableProperty] private string _sortByText = "Sort By: Product Name";
        [ObservableProperty] private bool _showTotals = true;
        [ObservableProperty] private bool _showDiscount = true;
        [ObservableProperty] private bool _isOrderSummaryExpanded = true;
        [ObservableProperty] private bool _canEdit = true;
        [ObservableProperty] private bool _showSendButton = true;
        [ObservableProperty] private bool _showAddCredit = false;
        [ObservableProperty] private string _actionButtonsColumnDefinitions = "*,*,*,*,*";

        public bool ShowTotalInHeader => ShowTotals && !IsOrderSummaryExpanded;
        partial void OnIsOrderSummaryExpandedChanged(bool value) => OnPropertyChanged(nameof(ShowTotalInHeader));
        partial void OnShowAddCreditChanged(bool value) => ActionButtonsColumnDefinitions = value ? "*,*,*,*,*" : "*,*,*,*";

        #endregion

        public NewOrderTemplatePageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowTotals = !Config.HidePriceInTransaction;
            MessagingCenter.Subscribe<SortByDialogViewModel, Tuple<SortDetails.SortCriteria, bool>>(this,
                "SortCriteriaApplied", (sender, args) => ApplySortCriteria(args.Item1, args.Item2));
        }

        public virtual void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            int? orderId = null;
            bool asPresale = false;
            if (query.TryGetValue("orderId", out var orderValue) && orderValue != null && int.TryParse(orderValue.ToString(), out var oId))
                orderId = oId;
            if (query.TryGetValue("asPresale", out var presaleValue) && presaleValue != null && int.TryParse(presaleValue.ToString(), out var presale))
                asPresale = presale == 1;
            if (orderId.HasValue)
            {
                _pendingOrderId = orderId;
                _pendingAsPresale = asPresale;
                MainThread.BeginInvokeOnMainThread(async () => await InitializeAsync(orderId.Value, asPresale));
            }
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
                return;
            _asPresale = asPresale;
            _initialized = true;
            _sortCriteria = SortDetails.GetCriteriaFromName(Config.PrintInvoiceSort);
            SortByText = $"Sort By: {GetSortCriteriaName(_sortCriteria)}";
            PrepareList();
        }

        private string GetSortCriteriaName(SortDetails.SortCriteria criteria)
        {
            return criteria switch
            {
                SortDetails.SortCriteria.ProductName => "Product Name",
                SortDetails.SortCriteria.ProductCode => "Product Code",
                SortDetails.SortCriteria.Category => "By Category",
                SortDetails.SortCriteria.InStock => "In Stock",
                SortDetails.SortCriteria.Qty => "Qty",
                SortDetails.SortCriteria.Descending => "Descending",
                SortDetails.SortCriteria.OrderOfEntry => "Order of Entry",
                SortDetails.SortCriteria.WarehouseLocation => "Warehouse Location",
                SortDetails.SortCriteria.CategoryThenByCode => "Category then by Code",
                _ => "Product Name"
            };
        }

        private void ApplySortCriteria(SortDetails.SortCriteria criteria, bool justOrdered)
        {
            _sortCriteria = criteria;
            SortByText = $"Sort By: {GetSortCriteriaName(_sortCriteria)}";
            SyncLinesWithOrder();
        }

        /// <summary>Build template lines from client ordered list + order details (same as Xamarin NewOrderTemplateActivity.PrepareList). Override in credit template to skip history.</summary>
        protected virtual void PrepareList()
        {
            _lines.Clear();
            if (_order == null) return;

            var isVendor = _order.Client?.ExtraProperties?.FirstOrDefault(x => x != null && string.Equals(x.Item1, "VENDOR", StringComparison.OrdinalIgnoreCase)) is { } vendor
                && string.Equals(vendor.Item2, "YES", StringComparison.OrdinalIgnoreCase);
            if (isVendor)
            {
                SyncLinesWithOrder();
                return;
            }

            if (FromCreditTemplate)
            {
                SyncLinesWithOrder();
                return;
            }

            var source = new List<LastTwoDetails>();

            if (!Config.LeftOrderTemplateEmpty)
            {
                _order.Client.EnsurePreviouslyOrdered();
                if (_order.Client.OrderedList != null)
                    source = _order.Client.OrderedList.ToList();

                if (Config.PopulateTemplateAuthProd)
                {
                    IEnumerable<Product> products = _order.Client.CategoryId > 0
                        ? ClientCategoryProducts.Find(_order.Client.CategoryId).Products
                        : Product.Products;
                    foreach (var p in products ?? Enumerable.Empty<Product>())
                    {
                        if (p.GetInventory(_order.AsPresale) <= 0)
                            continue;
                        if (source.Any(x => x?.Last != null && x.Last.ProductId == p.ProductId))
                            continue;
                        var item = new LastTwoDetails
                        {
                            Last = new InvoiceDetail
                            {
                                ClientId = _order.Client.ClientId,
                                Date = DateTime.MaxValue,
                                InvoiceId = 0,
                                Price = 0,
                                ProductId = p.ProductId,
                                Quantity = 0
                            },
                            Previous = null
                        };
                        source.Add(item);
                    }
                }
            }

            List<int> availableProducts = null;
            if (_order.Client.CategoryId != 0)
                availableProducts = ClientCategoryProducts.Find(_order.Client.CategoryId).Products?.Select(x => x.ProductId).ToList();

            foreach (var detail in source)
            {
                if (detail?.Last?.Product == null)
                    continue;
                if (detail.Last.Quantity < 0)
                    continue;
                if (detail.Last.Product.CategoryId == 0)
                    continue;
                if (availableProducts != null && !availableProducts.Contains(detail.Last.Product.ProductId))
                    continue;

                TemplateLine line;
                if (detail.Last.Product.SoldByWeight || detail.Last.Product.UseLot || detail.Last.Product.UseLotAsReference)
                    line = new GroupedTemplateLine();
                else
                    line = new StandartTemplateLine();

                line.Product = detail.Last.Product;
                line.AsPresale = _order.AsPresale;
                line.IsCredit = false;
                line.Damaged = false;

                if (detail.Last.Quantity > 0)
                {
                    line.PreviouslyOrdered = true;
                    line.PreviouslyOrderedPrice = Math.Round(detail.Last.Price, Config.Round);
                    line.PreviouslyOrderedUoM = UnitOfMeasure.List?.FirstOrDefault(x => x.Id == detail.Last.UnitOfMeasureId);
                    line.PreviouslyOrderedQty = (float)detail.Last.Quantity;
                    line.PerWeek = detail.PerWeek;
                    line.LastVisit = detail.Last.Date;
                }

                double expectedPrice = Product.GetPriceForProduct(line.Product, _order, false, false);
                line.ExpectedPrice = expectedPrice;

                double price = 0;
                if (Offer.ProductHasSpecialPriceForClient(line.Product, _order.Client, out price))
                {
                    line.Price = price;
                    line.IsPriceFromSpecial = true;
                }
                else
                {
                    line.Price = expectedPrice;
                    line.IsPriceFromSpecial = false;
                }

                if (!string.IsNullOrEmpty(line.Product.UoMFamily))
                {
                    if (Config.UseLastUoM && detail.Last.UnitOfMeasureId > 0 && line.PreviouslyOrderedUoM != null)
                    {
                        line.ExpectedPrice = line.ExpectedPrice * line.PreviouslyOrderedUoM.Conversion;
                        line.Price = line.Price * line.PreviouslyOrderedUoM.Conversion;
                        line.UoM = line.PreviouslyOrderedUoM;
                    }
                    else if (Config.UseLastUoM && detail.Last.UnitOfMeasureId > 0)
                    {
                        Logger.CreateLog("bad data, last detail with uomid=" + detail.Last.UnitOfMeasureId + " and UoM not found");
                    }
                    else
                    {
                        var defaultUoM = line.Product.UnitOfMeasures?.FirstOrDefault(x => x.IsDefault);
                        if (defaultUoM != null)
                        {
                            line.ExpectedPrice = line.ExpectedPrice * defaultUoM.Conversion;
                            line.Price = line.Price * defaultUoM.Conversion;
                            line.UoM = defaultUoM;
                        }
                        else
                        {
                            Logger.CreateLog("bad data, UOM without default");
                        }
                    }
                }

                if (Config.HidePriceInTransaction)
                {
                    line.Price = 0;
                    line.ExpectedPrice = 0;
                }

                _lines.Add(line);
            }

            SyncLinesWithOrder();
        }

        /// <summary>Details to merge into template lines. Override in credit template to only include IsCredit details.</summary>
        protected virtual IEnumerable<OrderDetail> GetOrderDetailsForSync()
        {
            return _order?.Details ?? Enumerable.Empty<OrderDetail>();
        }

        private void SyncLinesWithOrder()
        {
            if (_order == null) return;
            foreach (var orderDetail in GetOrderDetailsForSync())
            {
                var line = _lines.FirstOrDefault(x => IsCompatible(orderDetail, x));
                if (line == null)
                {
                    TemplateLine newLine;
                    if (orderDetail.Product.SoldByWeight || orderDetail.Product.UseLot || orderDetail.Product.UseLotAsReference)
                        newLine = new GroupedTemplateLine();
                    else
                        newLine = new StandartTemplateLine();
                    newLine.Product = orderDetail.Product;
                    newLine.AsPresale = _order.AsPresale;
                    newLine.IsCredit = orderDetail.IsCredit;
                    newLine.Damaged = orderDetail.Damaged;
                    if (newLine is StandartTemplateLine stl)
                    {
                        newLine.UoM = orderDetail.UnitOfMeasure;
                        newLine.ExpectedPrice = orderDetail.ExpectedPrice;
                        double price = 0;
                        if (Offer.ProductHasSpecialPriceForClient(newLine.Product, _order.Client, out price, newLine.UoM))
                            newLine.IsPriceFromSpecial = Math.Abs(orderDetail.Price - price) < 0.001;
                        else
                            newLine.IsPriceFromSpecial = false;
                        stl.Detail = orderDetail;
                    }
                    else
                    {
                        var gl = (GroupedTemplateLine)newLine;
                        newLine.UoM = newLine.Product.UnitOfMeasures?.FirstOrDefault(x => x.IsDefault);
                        newLine.ExpectedPrice = Product.GetPriceForProduct(newLine.Product, _order, false, false);
                        double price = 0;
                        if (Offer.ProductHasSpecialPriceForClient(newLine.Product, _order.Client, out price))
                        {
                            newLine.Price = price;
                            newLine.IsPriceFromSpecial = true;
                        }
                        else
                        {
                            newLine.Price = newLine.ExpectedPrice;
                            newLine.IsPriceFromSpecial = false;
                        }
                        if (newLine.UoM != null)
                        {
                            newLine.ExpectedPrice *= newLine.UoM.Conversion;
                            newLine.Price *= newLine.UoM.Conversion;
                        }
                        gl.Details.Add(orderDetail);
                    }
                    _lines.Add(newLine);
                }
                else
                {
                    if (line is StandartTemplateLine stl)
                    {
                        line.UoM = orderDetail.UnitOfMeasure;
                        line.ExpectedPrice = orderDetail.ExpectedPrice;
                        double price = 0;
                        if (Offer.ProductHasSpecialPriceForClient(line.Product, _order.Client, out price, line.UoM))
                            line.IsPriceFromSpecial = Math.Abs(orderDetail.Price - price) < 0.001;
                        else
                            line.IsPriceFromSpecial = false;
                        stl.Detail = orderDetail;
                    }
                    else
                        ((GroupedTemplateLine)line).Details.Add(orderDetail);
                }
            }
            _lines = SortDetails.SortedDetails(_lines).ToList();
            UpdateUI();
            BuildTemplateLineViewModels();
        }

        private static bool IsCompatible(OrderDetail orderDetail, TemplateLine line)
        {
            if (line.Product?.ProductId != orderDetail.Product?.ProductId) return false;
            if (line.IsCredit != orderDetail.IsCredit) return false;
            if (line.Damaged != orderDetail.Damaged) return false;
            return true;
        }

        protected virtual void UpdateUI()
        {
            if (_order == null) return;
            var details = _order.Details;
            ClientName = _order.Client?.ClientName ?? "";
            OrderTypeText = _order.AsPresale ? (_order.IsQuote ? "Quote" : "Sales Order") : "Sales Invoice";
            TermsText = "Terms: " + (_order.Term ?? "");
            TermsVisible = !string.IsNullOrEmpty(_order.Term);
            if (!string.IsNullOrEmpty(_order.CompanyName))
            {
                CompanyText = "Company: " + _order.CompanyName;
                ShowCompany = true;
            }
            else
                ShowCompany = false;
            LinesText = "Lines: " + details.Count;
            var totalQty = details.Sum(x => x.Qty);
            QtySoldText = "Qty Sold: " + totalQty;
            var subtotal = _order.CalculateItemCost();
            var discount = _order.CalculateDiscount();
            var tax = _order.CalculateTax();
            var total = _order.OrderTotalCost();
            var orderAmount = details.Where(x => !x.IsCredit).Sum(x => x.Qty * x.Price);
            var creditAmount = details.Where(x => x.IsCredit).Sum(x => x.Qty * x.Price);
            OrderAmountText = "Order: " + orderAmount.ToCustomString();
            CreditAmountText = "Credit: " + (creditAmount > 0 ? (creditAmount * -1).ToCustomString() : creditAmount.ToCustomString());
            SubtotalText = "Subtotal: " + subtotal.ToCustomString();
            DiscountText = "Discount: " + discount.ToCustomString();
            TaxText = "Tax: " + tax.ToCustomString();
            TotalText = "Total: " + total.ToCustomString();
            ShowDiscount = (_order.Client?.UseDiscount == true || _order.Client?.UseDiscountPerLine == true) || _order.IsDelivery;
            ShowSendButton = _order.AsPresale;
            ShowAddCredit = !_order.IsQuote && _order.OrderType == OrderType.Order && (_order.Client?.AllowOneDoc == true);
            CanEdit = !_order.Locked() && !_order.Dexed && !_order.Finished && !_order.Voided;
        }

        private void BuildTemplateLineViewModels()
        {
            TemplateLines.Clear();
            foreach (var line in _lines)
            {
                var vm = new TemplateLineItemViewModel { Line = line, IsEnabled = CanEdit };
                vm.RefreshFromLine();
                TemplateLines.Add(vm);
            }
        }

        [RelayCommand]
        private void ToggleOrderSummary() => IsOrderSummaryExpanded = !IsOrderSummaryExpanded;

        [RelayCommand]
        private async Task LineTapped(TemplateLineItemViewModel? item)
        {
            if (item?.Line == null || _order == null) return;
            await LineAddOrEditAsync(item);
        }

        [RelayCommand]
        private async Task LineAddOrEdit(TemplateLineItemViewModel? item)
        {
            if (item == null || _order == null) return;
            await LineAddOrEditAsync(item);
        }

        private async Task LineAddOrEditAsync(TemplateLineItemViewModel item)
        {
            if (_order == null || item?.Line == null) return;
            var tl = item.Line as TemplateLine;
            if (tl?.Product == null) return;
            int itemType = tl.IsCredit ? (tl.Damaged ? 1 : 2) : 0;
            await Shell.Current.GoToAsync(
                $"newordertemplatedetails?orderId={_order.OrderId}&productId={tl.Product.ProductId}&itemType={itemType}&fromCreditTemplate={(FromCreditTemplate ? 1 : 0)}");
        }

        [RelayCommand]
        private async Task AddCredit()
        {
            if (_order == null) return;
            await Shell.Current.GoToAsync($"newcredittemplate?orderId={_order.OrderId}&asPresale={(_asPresale ? 1 : 0)}&fromOrderTemplate=1");
        }

        [RelayCommand]
        private async Task ViewProducts()
        {
            if (_order == null) return;
            // Simple product list (name + OH); product select → NewOrderTemplateDetailsPage
            await Shell.Current.GoToAsync($"fullproductlist?orderId={_order.OrderId}&asPresale={(_asPresale ? 1 : 0)}&fromCreditTemplate={(FromCreditTemplate ? 1 : 0)}");
        }

        [RelayCommand]
        private async Task ViewCategories()
        {
            if (_order == null) return;
            // Categories first; category select → FullProductListPage; product select → NewOrderTemplateDetailsPage
            await Shell.Current.GoToAsync($"fullcategory?orderId={_order.OrderId}&asPresale={(_asPresale ? 1 : 0)}&comingFrom={(FromCreditTemplate ? "CreditTemplate" : "OrderTemplate")}&asCreditItem={(FromCreditTemplate ? 1 : 0)}");
        }

        [RelayCommand]
        private async Task Search()
        {
            if (_order == null) return;
            await Shell.Current.GoToAsync($"advancedcatalog?orderId={_order.OrderId}&asPresale={(_asPresale ? 1 : 0)}&search=1");
        }

        [RelayCommand]
        private async Task SortBy()
        {
            await Shell.Current.GoToAsync($"sortbydialog?source=NewOrderTemplatePageViewModel&sortCriteria={_sortCriteria}&justOrdered=false");
        }

        [RelayCommand]
        private async Task SendOrder()
        {
            if (_order == null) return;
            bool isEmpty = _order.Details.Count == 0 ||
                (_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem);
            if (isEmpty)
            {
                await _dialogService.ShowAlertAsync("You can't send an empty order", "Alert");
                return;
            }
            await _dialogService.ShowAlertAsync("Use the order menu to finalize and send the order.", "Send Order");
            SyncLinesWithOrder();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized && _pendingOrderId.HasValue)
                await InitializeAsync(_pendingOrderId.Value, _pendingAsPresale);
            if (!_initialized || _order == null) return;
            _order.Latitude = Config.LastLatitude;
            _order.Longitude = Config.LastLongitude;
            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            if (_order == null) return;
            CanEdit = !_order.Locked() && !_order.Dexed && !_order.Finished && !_order.Voided;
            ShowSendButton = _order.AsPresale;
            ShowAddCredit = !_order.IsQuote && _order.OrderType == OrderType.Order && (_order.Client?.AllowOneDoc == true);
            PrepareList();
            await Task.CompletedTask;
        }

        protected void UpdateRoute(bool close)
        {
            if (!Config.CloseRouteInPresale || _order == null)
                return;
            var stop = RouteEx.Routes.FirstOrDefault(x =>
                x.Date.Date == DateTime.Today &&
                x.Client != null &&
                x.Client.ClientId == _order.Client.ClientId);
            if (stop != null)
            {
                if (close)
                    stop.AddOrderToStop(_order.UniqueId);
                else
                    stop.RemoveOrderFromStop(_order.UniqueId);
            }
        }

        /// <summary>Finalize order before leaving (mirrors NewOrderTemplateActivity.FinalizeOrder). Returns true if caller should navigate back.</summary>
        protected virtual async Task<bool> FinalizeOrderAsync()
        {
            if (_order == null) return true;

            if (_order.Voided)
            {
                return true;
            }

            bool isEmpty = _order.Details.Count == 0 ||
                (_order.Details.Count == 1 && _order.Details[0].Product.ProductId == Config.DefaultItem);

            if (isEmpty)
            {
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
                    return true;
                }

                var result = await _dialogService.ShowConfirmAsync(
                    "You have to set all quantities to zero. Do you want to void this order?", "Alert", "Yes", "No");
                if (result)
                {
                    Logger.CreateLog($"Order with id={_order.OrderId} VOIDED from template");
                    if (_order.IsDelivery && string.IsNullOrEmpty(_order.PrintedOrderId))
                        _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);
                    _order.Finished = true;
                    _order.Void();
                    _order.Save();
                    return true;
                }
                return false;
            }

            // Non-empty order: validations then finalize
            if (!_order.AsPresale)
            {
                foreach (var detail in _order.Details)
                {
                    if (detail.Product.SoldByWeight && detail.Weight == 0)
                    {
                        await _dialogService.ShowAlertAsync("Weight items must have weight.", "Warning");
                        return false;
                    }
                }
            }

            if (Config.SetPO && string.IsNullOrEmpty(_order.PONumber) && _order.Client?.POIsMandatory == true)
            {
                await _dialogService.ShowAlertAsync("You need to enter PO number.", "Alert");
                return false;
            }
            if (_order.OrderType == OrderType.Bill && string.IsNullOrEmpty(_order.PONumber) && Config.BillNumRequired)
            {
                await _dialogService.ShowAlertAsync("You need to enter Bill number.", "Alert");
                return false;
            }
            if (_order.AsPresale && Config.ShipDateIsMandatory && _order.ShipDate.Year == 1)
            {
                await _dialogService.ShowAlertAsync("Please select ship date.", "Alert");
                return false;
            }
            if (_order.AsPresale && Config.SendOrderIsMandatory)
            {
                await _dialogService.ShowAlertAsync("Order must be sent.", "Alert");
                return false;
            }
            if (Config.MustEnterCaseInOut && (_order.OrderType == OrderType.Order || _order.OrderType == OrderType.Credit) && !_order.AsPresale)
            {
                await _dialogService.ShowAlertAsync("Case in/out functionality is not yet fully implemented.", "Info");
                return false;
            }

            if (Session.session != null)
                Session.session.AddDetailFromOrder(_order);

            if (_order.EndDate == DateTime.MinValue)
            {
                _order.EndDate = DateTime.Now;
                _order.Save();
            }

            if (_order.AsPresale)
            {
                UpdateRoute(true);
                if (Config.GeneratePresaleNumber && string.IsNullOrEmpty(_order.PrintedOrderId))
                {
                    _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);
                    _order.Save();
                }
            }

            if (Config.LotIsMandatoryBeforeFinalize && _order.Details.Any(x => string.IsNullOrEmpty(x.Lot)))
            {
                await _dialogService.ShowAlertAsync("Lot is mandatory.", "Alert");
                return false;
            }

            return true;
        }

        /// <summary>Called after FinalizeOrderAsync returns true. Override in credit to navigate to order details when Order/Bill.</summary>
        protected virtual async Task NavigateAfterFinalizeAsync()
        {
            Helpers.NavigationHelper.RemoveNavigationState(GetTemplateRouteName());
            await Shell.Current.GoToAsync("..");
        }

        public virtual async Task GoBackAsync()
        {
            bool shouldClose = await FinalizeOrderAsync();
            if (!shouldClose) return;
            await NavigateAfterFinalizeAsync();
        }

        public async Task HandleScannedBarcodeAsync(string data)
        {
            if (_order == null || string.IsNullOrEmpty(data) || !CanEdit) return;
            var product = ActivityExtensionMethods.GetProduct(_order, data);
            if (product == null)
            {
                await _dialogService.ShowAlertAsync("Product not found for scanned barcode.", "Info");
                return;
            }
            var lineVm = TemplateLines.FirstOrDefault(x => x.Line is TemplateLine t && t.Product?.ProductId == product.ProductId && !t.IsCredit)
                ?? TemplateLines.FirstOrDefault(x => x.Line is TemplateLine t && t.Product?.ProductId == product.ProductId);
            if (lineVm != null)
                await LineAddOrEditAsync(lineVm);
            else
            {
                int itemType = 0;
                if (FromCreditTemplate)
                {
                    var choice = await _dialogService.ShowActionSheetAsync("Add as", null, "Cancel", "Dump", "Return");
                    if (string.IsNullOrEmpty(choice) || choice == "Cancel") return;
                    itemType = choice == "Dump" ? 1 : 2;
                }
                await Shell.Current.GoToAsync(
                    $"newordertemplatedetails?orderId={_order.OrderId}&productId={product.ProductId}&itemType={itemType}&fromCreditTemplate={(FromCreditTemplate ? 1 : 0)}");
            }
            SyncLinesWithOrder();
        }

        public async Task HandleScannedQRCodeAsync(BarcodeDecoder decoder)
        {
            if (_order == null || decoder == null || !CanEdit) return;
            if (!string.IsNullOrEmpty(decoder.UPC))
                await HandleScannedBarcodeAsync(decoder.UPC);
            else if (!string.IsNullOrEmpty(decoder.Data))
                await HandleScannedBarcodeAsync(decoder.Data);
            else
                await _dialogService.ShowAlertAsync("No product data in QR code.", "Info");
        }

        public List<MenuOption> BuildMenuOptions()
        {
            var options = new List<MenuOption>();
            if (_order == null) return options;
            if (_order.AsPresale)
            {
                options.Add(new MenuOption("Set Ship Date", async () => { await _dialogService.ShowAlertAsync("Set Ship Date – use order menu from order details.", "Info"); SyncLinesWithOrder(); }));
                options.Add(new MenuOption(_order.OrderType == OrderType.Bill ? "Set Bill Number" : "Set PO", async () => { await _dialogService.ShowAlertAsync("Set PO – use order menu from order details.", "Info"); SyncLinesWithOrder(); }));
                options.Add(new MenuOption("Add Comments", async () => { await _dialogService.ShowAlertAsync("Add Comments – use order menu from order details.", "Info"); SyncLinesWithOrder(); }));
                options.Add(new MenuOption("Ordered By", async () => { await Shell.Current.GoToAsync($"ordersignature?orderId={_order.OrderId}"); SyncLinesWithOrder(); }));
                options.Add(new MenuOption("Send", async () => { await SendOrderCommand.ExecuteAsync(null); }));
                options.Add(new MenuOption("Select Driver", async () => { await _dialogService.ShowAlertAsync("Select Driver – use order menu from order details.", "Info"); SyncLinesWithOrder(); }));
            }
            else
            {
                options.Add(new MenuOption("Add Comments", async () => { await _dialogService.ShowAlertAsync("Add Comments – use order menu from order details.", "Info"); SyncLinesWithOrder(); }));
                options.Add(new MenuOption(_order.OrderType == OrderType.Bill ? "Set Bill Number" : "Set PO", async () => { await _dialogService.ShowAlertAsync("Set PO – use order menu from order details.", "Info"); SyncLinesWithOrder(); }));
            }
            options.Add(new MenuOption("Print", async () =>
            {
                if (_order == null) return;
                try
                {
                    await _dialogService.ShowLoadingAsync("Generating PDF...");
                    string pdfFile = PdfHelper.GetOrderPdf(_order);
                    await _dialogService.HideLoadingAsync();
                    if (!string.IsNullOrEmpty(pdfFile))
                        await Shell.Current.GoToAsync($"pdfviewer?pdfPath={Uri.EscapeDataString(pdfFile)}&orderId={_order.OrderId}");
                    else
                        await _dialogService.ShowAlertAsync("Error generating PDF.", "Alert", "OK");
                }
                catch (Exception ex)
                {
                    await _dialogService.HideLoadingAsync();
                    Logger.CreateLog(ex.ToString());
                    await _dialogService.ShowAlertAsync("Error occurred.", "Alert", "OK");
                }
            }));
            options.Add(new MenuOption("Delete", async () =>
            {
                var confirm = await _dialogService.ShowConfirmAsync("Delete this order?", "Delete", "Yes", "No");
                if (confirm && _order != null) { _order.Void(); _order.Save(); await GoBackAsync(); }
            }));
            return options;
        }
    }
}
