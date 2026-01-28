using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LaceupMigration.Business.Interfaces;

namespace LaceupMigration.ViewModels
{
    public partial class TransferOnOffPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private readonly AdvancedOptionsService _advancedOptionsService;
        
        private readonly ICameraBarcodeScannerService _cameraBarcodeScanner;

        private LaceupMigration.TransferAction _transferAction;
        private bool _changed;
        private bool _currentlyDisplayingAll = false; // Matches Xamarin: starts as false so button shows "Changed" and we show all items initially
        private string _comment = string.Empty;
        private string _tempFile = string.Empty;
        private List<TransferLine> _allProductList = new();
        private int _lastDetailId = 0;

        public bool Changed
        {
            get => _changed;
            set => SetProperty(ref _changed, value);
        }

        public LaceupMigration.TransferAction TransferAction => _transferAction;

        [ObservableProperty] private ObservableCollection<TransferLineViewModel> _transferLines = new();
        [ObservableProperty] private string _totalText = string.Empty;
        [ObservableProperty] private bool _readOnly;
        [ObservableProperty] private string _title = "Transfer";
        [ObservableProperty] private string _filterButtonText = "Changed";
        [ObservableProperty] private string _searchQuery = string.Empty;
        [ObservableProperty] private string _sortButtonText = "Sort By: Product Name";

        public TransferOnOffPageViewModel(DialogService dialogService, ILaceupAppService appService, AdvancedOptionsService advancedOptionsService, ICameraBarcodeScannerService cameraBarcodeScanner)
        {
            _dialogService = dialogService;
            _appService = appService;
            _advancedOptionsService = advancedOptionsService;
            _cameraBarcodeScanner = cameraBarcodeScanner;
        }

        public async Task InitializeAsync(string action)
        {
            _transferAction = action == "transferOn" ? LaceupMigration.TransferAction.On : LaceupMigration.TransferAction.Off;
            Title = _transferAction == LaceupMigration.TransferAction.On ? "Transfer On" : "Transfer Off";
            _tempFile = Path.Combine(Config.DataPath, _transferAction.ToString() + "_temp_LoadOrderPath.xml");
            
            // [ACTIVITY STATE]: Check if restoring from state and use saved temp file path
            // Match Xamarin TransferActivity: loads temp file path from ActivityState.State
            var state = LaceupMigration.ActivityState.GetState("TransferOnOffActivity");
            if (state != null && state.State != null && state.State.ContainsKey("tempFilePath"))
            {
                var savedTempFilePath = state.State["tempFilePath"];
                if (!string.IsNullOrEmpty(savedTempFilePath) && File.Exists(savedTempFilePath))
                {
                    // Use the saved temp file path from ActivityState
                    _tempFile = savedTempFilePath;
                }
            }
            
            UpdateSortButtonText();
            await LoadInventoryAsync();
        }

        public List<MenuOption> BuildMenuOptions()
        {
            var options = new List<MenuOption>();

            // Submit - Match Xamarin: always visible UNLESS file doesn't exist
            var finalFile = GetFinalFilePath();
            if (File.Exists(finalFile) || !ReadOnly)
            {
                options.Add(new MenuOption("Submit", async () => await SubmitAsync()));
            }

            options.Add(new MenuOption("Add Comments", async () => await ShowCommentsDialog()));

            if (!ReadOnly)
                options.Add(new MenuOption("Delete Document", async () => await DeleteDocumentAsync()));

            return options;
        }
        
        /// <summary>
        /// Gets the current temp file path. Used for saving to ActivityState.
        /// </summary>
        public string GetTempFilePath()
        {
            return _tempFile;
        }

        private async Task LoadInventoryAsync()
        {
            await Task.Run(() =>
            {
                // If ReadOnly and we already have data with details, preserve the details
                bool preserveDetails = ReadOnly && _allProductList.Any(x => x.Details.Count > 0);
                var preservedDetails = new Dictionary<int, List<TransferLineDet>>();
                
                if (preserveDetails)
                {
                    // Preserve existing details
                    foreach (var line in _allProductList)
                    {
                        if (line.Details.Count > 0)
                        {
                            preservedDetails[line.Product.ProductId] = new List<TransferLineDet>(line.Details);
                        }
                    }
                }

                _allProductList = new List<TransferLine>();

                foreach (var item in Product.Products.Where(x => x.CategoryId > 0 && x.ProductType == ProductType.Inventory))
                {
                    if (_transferAction == LaceupMigration.TransferAction.Off && item.CurrentInventory <= 0 && !Config.CanGoBelow0)
                        continue;

                    var uom = UnitOfMeasure.List.FirstOrDefault(x => x.FamilyId == item.UoMFamily && x.IsBase);
                    
                    if (Config.ButlerCustomization)
                        uom = item.UnitOfMeasures.FirstOrDefault(x => x.IsDefault);

                    var transferLine = new TransferLine
                    {
                        Product = item,
                        UoM = uom
                    };
                    
                    // Restore preserved details if ReadOnly
                    if (preserveDetails && preservedDetails.ContainsKey(item.ProductId))
                    {
                        transferLine.Details = preservedDetails[item.ProductId];
                    }
                    
                    _allProductList.Add(transferLine);
                }

                // Load from temp file if exists (only if not ReadOnly or no preserved details)
                if (File.Exists(_tempFile) && (!ReadOnly || !preserveDetails))
                {
                    LoadList();
                    Changed = true;
                }

                // Apply sorting - match Xamarin RefreshList logic: productList = SortDetails.SortedDetails(productList).ToList();
                _allProductList = SortDetails.SortedDetails(_allProductList).ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Use FilterProducts to apply both filter toggle and search query
                    FilterProducts();
                    UpdateTotal();
                });
            });
        }

        private void RefreshTransferLines(List<TransferLine> source)
        {
            TransferLines.Clear();
            foreach (var line in source)
            {
                var viewModel = new TransferLineViewModel(line, this);
                TransferLines.Add(viewModel);
            }
        }

        public void UpdateTotal()
        {
            // Match Xamarin: calculate total value (price * qty * uom conversion)
            double total = 0;
            foreach (var line in _allProductList)
            {
                foreach (var detail in line.Details)
                {
                    var price = detail.Qty * line.Product.PriceLevel0;
                    if (detail.UoM != null)
                        price *= detail.UoM.Conversion;
                    total += price;
                }
            }

            if (!Config.Wstco)
                TotalText = $"Transfer Value: {total.ToCustomString()}";
            else
                TotalText = string.Empty;
        }

        #region ScanWithCamera

        [RelayCommand]
        private async Task ScanAsync()
        {
            if (ReadOnly)
                return;

            try
            {
                var scanResult = await _cameraBarcodeScanner.ScanBarcodeAsync();
                if (string.IsNullOrEmpty(scanResult))
                    return;

                // Find product by barcode (check UPC, SKU, Code) - match Xamarin FindScannedProduct logic
                var product = Product.Products.FirstOrDefault(p =>
                    (!string.IsNullOrEmpty(p.Upc) && p.Upc.Equals(scanResult, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Sku) && p.Sku.Equals(scanResult, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrEmpty(p.Code) && p.Code.Equals(scanResult, StringComparison.OrdinalIgnoreCase)));

                // Match Xamarin FindScannedProduct logic: if product is null, show "xxxxxx is not assigned to any product"
                // This matches Xamarin ActivityExtensionMethods.FindScannedProduct line 256:
                // DisplayDialog(sender, sender.GetString(Resource.String.alert), (exists ? inventoryMessage : data + " " + sender.GetString(Resource.String.notAssigned)), ...)
                if (product == null)
                {
                    await _dialogService.ShowAlertAsync(
                        $"{scanResult} is not assigned to any product.",
                        "Alert",
                        "OK");
                    return;
                }

                await ScannerDoTheThingAsync(product);
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error scanning: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error scanning barcode.", "Error", "OK");
            }
        }

        private async Task ScannerDoTheThingAsync(Product product)
        {
            // Match Xamarin ScannerDoTheThing logic (lines 1108-1137)
            if (product != null)
            {
                // First check if product is in the currently visible list (TransferLines)
                var lineViewModel = TransferLines.FirstOrDefault(x => x.Product.ProductId == product.ProductId);
                
                if (lineViewModel == null)
                {
                    // Product not in visible list, check if it's in the full product list
                    var line = _allProductList.FirstOrDefault(x => x.Product.ProductId == product.ProductId);
                    if (line != null)
                    {
                        // Product exists but is not visible in current filter - show warning
                        await _dialogService.ShowAlertAsync(
                            $"Warning",
                            $"{product.Name} is not visible in the current list.",
                            "OK");
                        return;
                    }
                }

                if (lineViewModel != null)
                {
                    // Product found in visible list - increment/decrement quantity
                    // Match Xamarin logic (lines 1125-1128): Transfer On adds 1, Transfer Off subtracts 1
                    // In Xamarin: line.Real = line.Real + 1 (for On) or line.Real = line.Real - 1 (for Off)
                    // In MAUI, we work with Details collection, so we need to add/subtract from the total
                    float qtyChange = _transferAction == LaceupMigration.TransferAction.On ? 1 : -1;
                    
                    // Check if a detail already exists for this product (use first detail as default)
                    var existingDetail = lineViewModel.TransferLine.Details.FirstOrDefault();
                    
                    if (existingDetail != null)
                    {
                        // Add to existing quantity - match Xamarin: line.Real = line.Real + 1 or -1
                        existingDetail.Qty += qtyChange;
                        
                        // Update the ViewModel
                        var detailViewModel = lineViewModel.Details.FirstOrDefault(x => x.TransferLineDet == existingDetail);
                        if (detailViewModel != null)
                        {
                            detailViewModel.Qty = existingDetail.Qty;
                        }
                        
                        // If qty becomes 0, remove the detail (match Xamarin behavior where Real can be 0)
                        if (existingDetail.Qty == 0)
                        {
                            lineViewModel.TransferLine.Details.Remove(existingDetail);
                            if (detailViewModel != null)
                                lineViewModel.Details.Remove(detailViewModel);
                            
                            // If no details remain and filter is "Changed", remove from visible list
                            if (!_currentlyDisplayingAll && lineViewModel.TransferLine.Details.Count == 0)
                            {
                                FilterProducts();
                            }
                        }
                    }
                    else
                    {
                        // Create new detail with qty 1 or -1 (match Xamarin: initial Real = 0, then becomes 1 or -1)
                        var detail = new TransferLineDet
                        {
                            Product = product,
                            Qty = qtyChange,
                            UoM = lineViewModel.Uom,
                            Lot = "",
                            LotExp = DateTime.MinValue,
                            Weight = 0,
                            Id = _lastDetailId++
                        };

                        lineViewModel.TransferLine.Details.Add(detail);
                        
                        // Also add to ViewModel's Details collection for UI update
                        var detailViewModel = new TransferLineDetViewModel(detail, lineViewModel, this);
                        lineViewModel.Details.Add(detailViewModel);
                        
                        // If filter is "Changed" and this is the first detail, ensure item is visible
                        if (!_currentlyDisplayingAll && lineViewModel.TransferLine.Details.Count == 1)
                        {
                            // Item should already be visible since we found it in TransferLines
                            // But if it wasn't visible before, we need to refresh
                            var existingInList = TransferLines.FirstOrDefault(x => x.TransferLine == lineViewModel.TransferLine);
                            if (existingInList == null)
                            {
                                TransferLines.Add(lineViewModel);
                            }
                        }
                    }
                    
                    Changed = true;
                    SaveList();
                    UpdateTotal();
                    
                    // Match Xamarin SetViewToCurrentProduct - scroll to item (handled by CollectionView automatically)
                }
                else
                {
                    // Product not found in inventory - show warning
                    await _dialogService.ShowAlertAsync(
                        "Warning",
                        $"{product.Name} is not part of the inventory.",
                        "OK");
                }
            }
        }

        #endregion

        [RelayCommand]
        private async Task Save()
        {
            // Match Xamarin SaveClicked logic
            if (Changed && Config.TransferComment && string.IsNullOrEmpty(_comment))
            {
                await ShowCommentsDialog();
                return;
            }

            // Check password if required - match Xamarin NewTransferActivity logic
            if (Config.TransferPasswordAtSaving)
            {
                var correctPassword = _transferAction == LaceupMigration.TransferAction.On 
                    ? Config.TransferPassword 
                    : Config.TransferOffPassword;

                if (!string.IsNullOrEmpty(correctPassword))
                {
                    var password = await _dialogService.ShowPromptAsync(
                        _transferAction == LaceupMigration.TransferAction.On ? "Transfer On" : "Transfer Off",
                        "Enter Password",
                        "OK",
                        "Cancel",
                        "",
                        -1,
                        "",
                        Keyboard.Default);

                    if (password == null)
                        return;

                    if (string.Compare(password ?? "", correctPassword ?? "", StringComparison.CurrentCultureIgnoreCase) != 0)
                    {
                        await _dialogService.ShowAlertAsync("Invalid password", "Alert", "OK");
                        return;
                    }
                }
            }

            // Confirm save
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Warning",
                "This will prevent you from doing more changes, continue?",
                "Yes",
                "No");

            if (confirmed)
            {
                ApplyTransfer();
                SaveInFinalFile();

                ReadOnly = true;
                Changed = false;

                // Delete temp file after save (match Xamarin behavior)
                // Details are preserved in _allProductList and will be restored on reload if ReadOnly is true
                if (File.Exists(_tempFile))
                    File.Delete(_tempFile);

                // Refresh the list to show updated inventory - match Xamarin adapter.NotifyDataSetChanged()
                FilterProducts();
                
                // After save, notify all ViewModels that CurrentInventoryText has changed (inventory was updated by ApplyTransfer)
                foreach (var lineViewModel in TransferLines)
                {
                    lineViewModel.NotifyInventoryChanged();
                }

                await _dialogService.ShowAlertAsync("Transfer saved successfully", "Success", "OK");
            }
        }

        [RelayCommand]
        private async Task Print()
        {
            try
            {
                PrinterProvider.PrintDocument((int number) =>
                {
                    if (number < 1)
                        return "Please enter a valid number of copies.";

                    CompanyInfo.SelectedCompany = CompanyInfo.Companies.Count > 0 ? CompanyInfo.Companies[0] : null;
                    IPrinter printer = PrinterProvider.CurrentPrinter();
                    bool allGood = true;

                    // Convert TransferLine to InventoryLine
                    List<InventoryLine> lines = new List<InventoryLine>();
                    foreach (var product in _allProductList)
                    {
                        foreach (var item in product.Details)
                        {
                            var l = new InventoryLine()
                            {
                                Product = item.Product,
                                Lot = item.Lot,
                                UoM = item.UoM,
                                Real = item.Qty
                            };

                            var inv = product.Product.ProductInv?.TruckInventories?.FirstOrDefault(x => x.Lot == item.Lot);
                            if (inv != null)
                            {
                                l.Starting = inv.CurrentQty;
                                if (l.UoM != null)
                                    l.Starting /= l.UoM.Conversion;
                            }

                            lines.Add(l);
                        }
                    }

                    for (int i = 0; i < number; i++)
                    {
                        // Get site name
                        var siteName = string.Empty;
                        // Note: transferTruckSiteId would need to be stored in the ViewModel if needed
                        // For now, using empty string as default

                        bool result = printer.PrintTransferOnOff(lines, _transferAction == TransferAction.On, ReadOnly, _comment, siteName);

                        if (!result)
                            allGood = false;
                    }

                    if (!allGood)
                        return "Error printing transfer.";
                    return string.Empty;
                }, 2);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error printing: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        partial void OnSearchQueryChanged(string value)
        {
            // Real-time search filtering
            FilterProducts();
        }

        private void FilterProducts()
        {
            // Match Xamarin ChangeWhatIsListedClicked logic
            List<TransferLine> baseList;
            
            if (_currentlyDisplayingAll)
            {
                // Will toggle to show "All" button, so currently showing only changed items
                baseList = _allProductList.Where(x => x.Details.Count > 0).ToList();
            }
            else
            {
                // Will toggle to show "Changed" button, so currently showing all items
                baseList = _allProductList;
            }

            // Apply search filter if search query is not empty
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var searchLower = SearchQuery.ToLowerInvariant();
                baseList = baseList.Where(x =>
                    x.Product.Name.ToLowerInvariant().Contains(searchLower) ||
                    x.Product.Description.ToLowerInvariant().Contains(searchLower) ||
                    x.Product.Upc.ToLowerInvariant().Contains(searchLower) ||
                    x.Product.Sku.ToLowerInvariant().Contains(searchLower) ||
                    x.Product.Code.ToLowerInvariant().Contains(searchLower)
                ).ToList();
            }

            // Apply sorting - match Xamarin RefreshList logic
            baseList = SortDetails.SortedDetails(baseList).ToList();

            RefreshTransferLines(baseList);
        }

        [RelayCommand]
        private void ToggleFilter()
        {
            // Match Xamarin ChangeWhatIsListedClicked logic
            string text;

            if (_currentlyDisplayingAll)
            {
                text = "All";
            }
            else
            {
                text = "Changed";
            }

            FilterButtonText = text;
            _currentlyDisplayingAll = !_currentlyDisplayingAll;
            
            // Re-filter with current search query
            FilterProducts();
        }

        public async Task ShowAddQtyDialog(TransferLineViewModel lineViewModel)
        {
            // Match Xamarin ShowQtyDialog logic - single dialog with Qty and UoM together
            var result = await _dialogService.ShowTransferQtyDialogAsync(lineViewModel.Product.Name,
                lineViewModel.Product, "1", lineViewModel.Uom);

            if (result.qty == null) return; // User cancelled

            if (string.IsNullOrEmpty(result.qty) || !float.TryParse(result.qty, out var qty)) return;

            // Match Xamarin: use absolute value
            qty = Math.Abs(qty);

            // Use the selected UoM from the dialog
            UnitOfMeasure unit = result.selectedUoM ?? lineViewModel.Uom; // Default to line's UoM if none selected

            string lot = ""; // TODO: Add lot selection when implementing full dialog
            DateTime lotExp = DateTime.MinValue; // TODO: Add lot expiration when implementing full dialog

            // Calculate baseQty (converted to base UoM) - match Xamarin line 1718-1719
            double baseQty = qty;
            if (unit != null) baseQty = qty * unit.Conversion;

            // Match Xamarin validation logic for Transfer Off (line 1721-1728)
            if (_transferAction == LaceupMigration.TransferAction.Off)
            {
                if (baseQty > lineViewModel.Product.CurrentInventory - lineViewModel.TransferLine.QtyTransferred &&
                    !Config.CanGoBelow0)
                {
                    await _dialogService.ShowAlertAsync(
                        $"You cannot go below 0, you have only {lineViewModel.Product.CurrentInventory} to transfer off.",
                        "Alert", "OK");
                    return;
                }
            }

            // CRITICAL: Always work with the underlying _allProductList data structure
            // Find the TransferLine in _allProductList (the source of truth)
            var transferLine = _allProductList.FirstOrDefault(x => x.Product.ProductId == lineViewModel.Product.ProductId);
            
            if (transferLine == null)
            {
                // This shouldn't happen, but handle it gracefully
                await _dialogService.ShowAlertAsync("Product not found in inventory list", "Error", "OK");
                return;
            }

            // Check if detail with same UoM already exists in the underlying TransferLine
            var existingDetail = transferLine.Details.FirstOrDefault(x => x.UoM == unit);

            if (existingDetail != null)
            {
                // Update existing detail in underlying model
                existingDetail.Qty = qty;
                existingDetail.Lot = lot;
                existingDetail.LotExp = lotExp;
                existingDetail.Weight = transferLine.Weight;
            }
            else
            {
                // Create new detail in underlying model
                var newDetail = new TransferLineDet
                {
                    Product = lineViewModel.Product,
                    Qty = qty,
                    UoM = unit,
                    Lot = lot,
                    LotExp = lotExp,
                    Weight = transferLine.Weight,
                    Id = _lastDetailId++
                };
                
                transferLine.Details.Add(newDetail);
            }

            Changed = true;
            SaveList();

            // Refresh the ViewModels from the updated _allProductList
            MainThread.BeginInvokeOnMainThread(() =>
            {
                FilterProducts();
                UpdateTotal();
            });
        }

        public async Task ShowEditQtyDialog(TransferLineDetViewModel detailViewModel, TransferLineViewModel lineViewModel)
        {
            // Match Xamarin ShowEditQtyDialog logic - single dialog with Qty and UoM together
            var result = await _dialogService.ShowTransferQtyDialogAsync(
                detailViewModel.Product.Name,
                lineViewModel.Product,
                detailViewModel.Qty.ToString(),
                detailViewModel.Uom,
                "Save");

            if (result.qty == null)
                return; // User cancelled

            if (result.qty != null && float.TryParse(result.qty, out var qty))
            {
                // Match Xamarin line 1896: use absolute value
                qty = Math.Abs(qty);

                // CRITICAL: Always work with the underlying _allProductList data structure
                // Find the TransferLine in _allProductList (the source of truth)
                var transferLine = _allProductList.FirstOrDefault(x => x.Product.ProductId == lineViewModel.Product.ProductId);
                
                if (transferLine == null)
                {
                    // This shouldn't happen, but handle it gracefully
                    await _dialogService.ShowAlertAsync("Product not found in inventory list", "Error", "OK");
                    return;
                }

                // Find the detail in the underlying TransferLine.Details
                var underlyingDetail = transferLine.Details.FirstOrDefault(x => x.Id == detailViewModel.Id);
                
                if (underlyingDetail == null)
                {
                    // Try to find by matching properties if Id doesn't match
                    underlyingDetail = transferLine.Details.FirstOrDefault(x => 
                        x.UoM == detailViewModel.Uom && 
                        Math.Abs(x.Qty - detailViewModel.Qty) < 0.001f);
                }

                if (qty == 0)
                {
                    // Match Xamarin line 1904-1907: remove detail if qty is 0
                    if (underlyingDetail != null)
                    {
                        transferLine.Details.Remove(underlyingDetail);
                    }
                    // Also remove from ViewModel (will be refreshed by FilterProducts)
                }
                else
                {
                    // Use the selected UoM from the dialog
                    UnitOfMeasure selectedUoM = result.selectedUoM ?? detailViewModel.Uom;
                    bool uomChanged = selectedUoM != detailViewModel.Uom;
                    
                    // Match Xamarin validation logic for Transfer Off EXACTLY (line 1950-1957)
                    if (_transferAction == LaceupMigration.TransferAction.Off)
                    {
                        // Match Xamarin line 1910-1913 exactly - use var and *= operator
                        var oldQTy = detailViewModel.Qty;
                        var oldBaseQty = oldQTy;
                        if (detailViewModel.Uom != null)
                            oldBaseQty *= (float)detailViewModel.Uom.Conversion;

                        // Match Xamarin line 1915-1948 exactly
                        float baseQty = qty;
                        // Use selected UoM from dialog
                        if (selectedUoM != null)
                            baseQty = qty * (float)selectedUoM.Conversion;

                        // Match Xamarin line 1952 EXACTLY - use the exact same formula and types
                        // Formula: baseQty > CurrentInventory - QtyTransferred - oldBaseQty
                        float currentInventory = lineViewModel.Product.CurrentInventory;
                        float qtyTransferred = transferLine.QtyTransferred;
                        
                        if (baseQty > currentInventory - qtyTransferred - oldBaseQty && !Config.CanGoBelow0)
                        {
                            await _dialogService.ShowAlertAsync(
                                $"You cannot go below 0, you have only {lineViewModel.Product.CurrentInventory} to transfer off.",
                                "Alert",
                                "OK");
                            return;
                        }
                    }

                    // Handle UoM change: if UoM changed and a detail with new UoM already exists, merge them
                    if (uomChanged && underlyingDetail != null)
                    {
                        var existingDetailWithNewUom = transferLine.Details.FirstOrDefault(x => 
                            x.UoM == selectedUoM && x.Id != underlyingDetail.Id);
                        
                        if (existingDetailWithNewUom != null)
                        {
                            // Merge: update existing detail with new UoM and remove the old one
                            existingDetailWithNewUom.Qty = qty;
                            existingDetailWithNewUom.Lot = detailViewModel.Lot;
                            existingDetailWithNewUom.LotExp = detailViewModel.LotExp;
                            existingDetailWithNewUom.Weight = detailViewModel.Weight;
                            transferLine.Details.Remove(underlyingDetail);
                        }
                        else
                        {
                            // Just update the UoM
                            underlyingDetail.Qty = qty;
                            underlyingDetail.UoM = selectedUoM;
                            underlyingDetail.Lot = detailViewModel.Lot;
                            underlyingDetail.LotExp = detailViewModel.LotExp;
                            underlyingDetail.Weight = detailViewModel.Weight;
                        }
                    }
                    else if (underlyingDetail != null)
                    {
                        // Update the underlying detail model (UoM didn't change)
                        underlyingDetail.Qty = qty;
                        underlyingDetail.Lot = detailViewModel.Lot;
                        underlyingDetail.LotExp = detailViewModel.LotExp;
                        underlyingDetail.Weight = detailViewModel.Weight;
                    }
                    else
                    {
                        // If detail not found, create a new one (shouldn't happen, but handle it)
                        var newDetail = new TransferLineDet
                        {
                            Product = lineViewModel.Product,
                            Qty = qty,
                            UoM = selectedUoM,
                            Lot = detailViewModel.Lot,
                            LotExp = detailViewModel.LotExp,
                            Weight = detailViewModel.Weight,
                            Id = detailViewModel.Id > 0 ? detailViewModel.Id : _lastDetailId++
                        };
                        transferLine.Details.Add(newDetail);
                    }
                }

                Changed = true;
                SaveList();
                
                // Always refresh to ensure UI is in sync with underlying data
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    FilterProducts();
                    UpdateTotal();
                });
            }
        }

        private void ApplyTransfer()
        {
            // Match Xamarin ApplyTransfer logic
            if (ReadOnly)
                return;

            int factor = _transferAction == LaceupMigration.TransferAction.Off ? -1 : 1;

            foreach (var line in _allProductList)
            {
                // Ensure ProductInv is initialized before updating inventory
                var _ = line.Product.ProductInv;
                
                foreach (var item in line.Details)
                {
                    // Match Xamarin: UpdateInventory with factor (1 for Transfer On, -1 for Transfer Off)
                    line.Product.UpdateInventory(item.Qty, item.UoM, item.Lot, item.LotExp, factor, item.Weight);
                    
                    // Match Xamarin: AddTransferredInventory with factor (1 for Transfer On, -1 for Transfer Off)
                    line.Product.AddTransferredInventory(item.Qty, item.UoM, item.Lot, item.LotExp, _transferAction == LaceupMigration.TransferAction.On ? 1 : -1, item.Weight);
                }
            }

            ProductInventory.Save();
            Logger.CreateLog("Inventory saved");
        }

        private void SaveInFinalFile()
        {
            // Match Xamarin SaveInFinalFile logic
            if (Config.BranchSiteId == 0)
                return;

            var salesman = Salesman.List.FirstOrDefault(x => x.Id == Config.SalesmanId);
            if (salesman == null)
                return;

            int sourceSite = 0;
            int targetSite = 0;

            if (_transferAction == LaceupMigration.TransferAction.Off)
            {
                sourceSite = salesman.InventorySiteId;
                targetSite = Config.BranchSiteId;
            }
            else
            {
                sourceSite = Config.BranchSiteId;
                targetSite = salesman.InventorySiteId;
            }

            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    var transfer = new Transfer(_transferAction, sourceSite, targetSite, _comment);

                    foreach (var line in _allProductList)
                    {
                        foreach (var item in line.Details)
                        {
                            transfer.AddDetail(line.Product.ProductId, Math.Abs(item.Qty), item.UoM != null ? item.UoM.Id : 0, item.Lot, item.LotExp, item.Weight);
                        }
                    }

                    transfer.SaveInFile();
                }
                finally
                {
                }
            }
        }

        private void LoadList()
        {
            // Match Xamarin LoadList logic
            if (File.Exists(_tempFile))
            {
                using (StreamReader reader = new StreamReader(_tempFile))
                {
                    string line = reader.ReadLine();
                    _comment = line ?? string.Empty;

                    var listofIds = new List<int>();

                    while ((line = reader.ReadLine()) != null)
                    {
                        try
                        {
                            string[] parts = line.Split(new char[] { (char)20 });
                            int productID = Convert.ToInt32(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                            float qty = Convert.ToSingle(parts[1], System.Globalization.CultureInfo.InvariantCulture);

                            UnitOfMeasure uom = null;
                            if (parts.Length > 2)
                            {
                                var uomId = Convert.ToInt32(parts[2], System.Globalization.CultureInfo.InvariantCulture);
                                uom = UnitOfMeasure.List.FirstOrDefault(x => x.Id == uomId);
                            }

                            string lot = "";
                            if (parts.Length > 3)
                                lot = parts[3];

                            DateTime lotExp = DateTime.MinValue;
                            if (parts.Length > 4)
                                lotExp = new DateTime(Convert.ToInt64(parts[4]));

                            double weight = 0;
                            if (parts.Length > 5)
                                weight = Convert.ToDouble(parts[5]);

                            int id = 0;
                            if (parts.Length > 6)
                                Int32.TryParse(parts[6], out id);

                            listofIds.Add(id);

                            var product = _allProductList.FirstOrDefault(x => x.Product.ProductId == productID);
                            if (product != null)
                            {
                                var detail = new TransferLineDet
                                {
                                    Product = product.Product,
                                    Qty = qty,
                                    UoM = uom,
                                    Lot = lot,
                                    LotExp = lotExp,
                                    Weight = weight,
                                    Id = id
                                };
                                product.Details.Add(detail);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.CreateLog(ex);
                        }
                    }

                    var ordered = listofIds.OrderByDescending(x => x).ToList();
                    var lastId = ordered.FirstOrDefault();
                    if (ordered.Count > 0 && lastId > 0)
                        _lastDetailId = lastId + 1;
                }
            }
        }

        public void SaveList()
        {
            // Match Xamarin SaveList logic
            // Don't save if temp file path is not initialized
            if (string.IsNullOrEmpty(_tempFile))
                return;
                
            lock (FileOperationsLocker.lockFilesObject)
            {
                try
                {
                    if (File.Exists(_tempFile))
                        File.Delete(_tempFile);

                    using (StreamWriter writer = new StreamWriter(_tempFile, false))
                    {
                        writer.WriteLine(_comment);

                        foreach (var line in _allProductList)
                        {
                            foreach (var item in line.Details)
                            {
                                writer.Write(item.Product.ProductId);
                                writer.Write((char)20);
                                writer.Write(item.Qty);
                                writer.Write((char)20);
                                writer.Write(item.UoM != null ? item.UoM.Id : 0);
                                writer.Write((char)20);
                                writer.Write(item.Lot);
                                writer.Write((char)20);
                                writer.Write(item.LotExp.Ticks);
                                writer.Write((char)20);
                                writer.Write(item.Weight);
                                writer.Write((char)20);
                                writer.Write(item.Id);
                                writer.WriteLine();
                            }
                        }
                    }
                }
                finally
                {
                }
            }
        }

        private async Task ShowCommentsDialog()
        {
            // Match Xamarin ShowCommentsDialog logic
            var commentText = await _dialogService.ShowPromptAsync(
                "Enter Transfer Reason",
                "Enter Transfer Reason",
                "OK",
                "Cancel",
                "",
                -1,
                _comment);

            if (!string.IsNullOrEmpty(commentText))
            {
                _comment = commentText;
                SaveList();
            }
        }

        public async Task ShowSortDialogAsync()
        {
            // Match Xamarin SortButton_Click logic
            // Create sort options from enum values (match Xamarin CreateSortOptions)
            var sortOptionsList = new List<SortDetails.SortCriteria>();
            foreach (var item in Enum.GetValues(typeof(SortDetails.SortCriteria)))
            {
                sortOptionsList.Add((SortDetails.SortCriteria)item);
            }

            // Convert to display names (match Xamarin SortCriteriaName)
            var sortOptionNames = sortOptionsList.Select(x => GetSortCriteriaName(x)).ToArray();

            // Get current selection index (match Xamarin: int which = (int)GetCriteriaFromName(Config.PrintInvoiceSort))
            var currentCriteria = SortDetails.GetCriteriaFromName(Config.PrintInvoiceSort);
            var currentIndex = sortOptionsList.IndexOf(currentCriteria);
            if (currentIndex < 0) currentIndex = 0;

            var selected = await _dialogService.ShowActionSheetAsync(
                "Sort By",
                null,
                "Cancel",
                sortOptionNames);

            if (selected == "Cancel" || string.IsNullOrEmpty(selected))
                return;

            // Find the selected criteria by matching the display name
            var selectedIndex = Array.IndexOf(sortOptionNames, selected);
            if (selectedIndex >= 0 && selectedIndex < sortOptionsList.Count)
            {
                var criteria = sortOptionsList[selectedIndex];
                
                // Save the criteria (match Xamarin: SaveSortCriteria((SortCriteria)which))
                SortDetails.SaveSortCriteria(criteria);
                
                // Update button text (match Xamarin: sortButton.Text = GetString(Resource.String.sortBy) + SortCriteriaName(Config.PrintInvoiceSort))
                UpdateSortButtonText();
                
                // Refresh list (match Xamarin: RefreshList())
                FilterProducts();
            }
        }

        private void UpdateSortButtonText()
        {
            // Match Xamarin: GetString(Resource.String.sortBy) + SortCriteriaName(Config.PrintInvoiceSort)
            var criteria = SortDetails.GetCriteriaFromName(Config.PrintInvoiceSort);
            var criteriaName = GetSortCriteriaName(criteria);
            SortButtonText = $"Sort By: {criteriaName}";
        }

        private string GetSortCriteriaName(SortDetails.SortCriteria criteria)
        {
            // Match Xamarin SortCriteriaName method
            return criteria switch
            {
                SortDetails.SortCriteria.ProductName => "Product Name",
                SortDetails.SortCriteria.ProductCode => "Product Code",
                SortDetails.SortCriteria.Category => "Category",
                SortDetails.SortCriteria.InStock => "In Stock",
                SortDetails.SortCriteria.Qty => "Qty",
                SortDetails.SortCriteria.Descending => "Descending",
                SortDetails.SortCriteria.OrderOfEntry => "Order of Entry",
                SortDetails.SortCriteria.WarehouseLocation => "Warehouse Location",
                SortDetails.SortCriteria.CategoryThenByCode => "Category Then By Code",
                _ => "Product Name"
            };
        }

        [RelayCommand]
        private async Task ShowMenu()
        {
            var options = new List<string>();
            
            // Submit - Match Xamarin: always visible UNLESS file doesn't exist
            // File gets deleted after successful SendTransfer(), so Submit disappears
            var finalFile = GetFinalFilePath();
            if (File.Exists(finalFile) || !ReadOnly)
            {
                options.Add("Submit");
            }

            // Add Comments
            options.Add("Add Comments");
            
            // Delete Document - only if there are changes or document exists
            if (!ReadOnly)
            {
                options.Add("Delete Document");
            }
            
            // Advanced Options
            options.Add("Advanced Options");
            
            if (options.Count == 0)
                return;
            
            var choice = await _dialogService.ShowActionSheetAsync("Menu", "", "Cancel", options.ToArray());
            
            if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
                return;
            
            switch (choice)
            {
                case "Submit":
                    await SubmitAsync();
                    break;
                case "Add Comments":
                    await ShowCommentsDialog();
                    break;
                case "Delete Document":
                    await DeleteDocumentAsync();
                    break;
                case "Advanced Options":
                    await _advancedOptionsService.ShowAdvancedOptionsAsync();
                    break;
            }
        }
        
        private async Task SubmitAsync()
        {
            // Match Xamarin transferSubmit logic
            // Check if comments are required and missing
            if (Changed && Config.TransferComment && string.IsNullOrEmpty(_comment))
            {
                await ShowCommentsDialog();
                return;
            }
            
            // Show confirmation dialog - match Xamarin: Alert title, submitTransaction message
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Alert",
                "Submit transaction?",
                "Yes",
                "No");
            
            if (!confirmed)
                return;
            
            // Submit the transfer
            await SubmitTransfer();
        }

        private async Task SubmitTransfer()
        {
            // Show loading indicator during submission
            await _dialogService.ShowLoadingAsync("Sending all information...");

            string responseMessage = null;
            bool errorDownloadingData = false;

            // Match Xamarin: Run on background thread
            await Task.Run(() =>
            {
                try
                {
                    SaveInFinalFile();

                    if (_transferAction == TransferAction.On)
                        DataProvider.SendTransfer(Config.TransferOnFile);
                    else
                        DataProvider.SendTransfer(Config.TransferOffFile);

                    ApplyTransfer();
                }
                catch (Exception e)
                {
                    MainThread.BeginInvokeOnMainThread(async () => await _dialogService.HideLoadingAsync());

                    errorDownloadingData = true;
                    responseMessage = "Error submitting the cycle count" + System.Environment.NewLine + e.Message;

                    Logger.CreateLog(e);
                }
            });

    // Match Xamarin: Finally block logic - always runs on UI thread
    await MainThread.InvokeOnMainThreadAsync(async () =>
    {
        string title = "Alert";
        if (string.IsNullOrEmpty(responseMessage))
        {
            responseMessage = "Cycle Count submitted successfully.";
            title = "Success";
        }

        await _dialogService.HideLoadingAsync();

        // Match Xamarin: Show appropriate dialog based on success/error
        if (errorDownloadingData)
        {
            await _dialogService.ShowAlertAsync(responseMessage, title, "OK");
        }
        else
        {
            await _dialogService.ShowAlertAsync("Data successfully transmitted.", title, "OK");

            // Match Xamarin: Update state after successful submission
            ReadOnly = true;
            Changed = false;

            // Delete temp file if it exists
            if (File.Exists(_tempFile)) File.Delete(_tempFile);

            // Refresh the list - equivalent to adapter.NotifyDataSetChanged()
            FilterProducts();

            // Match Xamarin: Notify that ReadOnly changed so UI can update button states
            OnPropertyChanged(nameof(ReadOnly));
        }
    });
}
        
        private async Task DeleteDocumentAsync()
        {
            var confirmed = await _dialogService.ShowConfirmationAsync(
                "Delete Document",
                "Are you sure you want to delete this transfer document? All changes will be lost.",
                "Yes",
                "No");
            
            if (!confirmed)
                return;
            
            // Delete temp file
            if (File.Exists(_tempFile))
                File.Delete(_tempFile);
            
            // Delete final file if it exists
            var finalFile = GetFinalFilePath();
            if (File.Exists(finalFile))
                File.Delete(finalFile);
            
            // Clear all data
            _allProductList.Clear();
            TransferLines.Clear();
            Changed = false;
            _comment = string.Empty;
            ReadOnly = false;
            
            // Reload inventory
            await LoadInventoryAsync();
            
            await _dialogService.ShowAlertAsync("Document deleted successfully", "Success", "OK");
        }
        
        private string GetFinalFilePath()
        {
            return _transferAction == LaceupMigration.TransferAction.On 
                ? Config.TransferOnFile 
                : Config.TransferOffFile;
        }
    }

    public partial class TransferLineViewModel : ObservableObject
    {
        private readonly TransferOnOffPageViewModel? _parent;

        [ObservableProperty] private Product _product = null!;
        [ObservableProperty] private UnitOfMeasure? _uom;
        [ObservableProperty] private ObservableCollection<TransferLineDetViewModel> _details = new();

        public TransferLine TransferLine { get; set; } = null!;

        public TransferLineViewModel()
        {
        }

        public TransferLineViewModel(TransferLine line, TransferOnOffPageViewModel? parent = null)
        {
            _parent = parent;
            Product = line.Product;
            Uom = line.UoM;
            TransferLine = line;

            foreach (var det in line.Details)
            {
                Details.Add(new TransferLineDetViewModel(det, this, parent));
            }
        }

        public string ProductName => Product?.Name ?? "Unknown";
        
        public string StartingInventoryText
        {
            get
            {
                var startingInv = Math.Round(Product.CurrentInventory, Config.Round);
                if (_parent != null && _parent.ReadOnly)
                {
                    int factor = _parent.TransferAction == LaceupMigration.TransferAction.Off ? -1 : 1;
                    startingInv = Math.Round(Product.CurrentInventory - (TransferLine.QtyTransferred * factor), Config.Round);
                }

                if (Uom != null && !Uom.IsBase)
                    startingInv /= Uom.Conversion;

                return $"Starting Inv: {Math.Round(startingInv, 2)}";
            }
        }

        public Color StartingInventoryColor
        {
            get
            {
                // Match Xamarin: red if <= 0, otherwise blue (#017CBA)
                if (Product.CurrentInventory <= 0)
                    return Colors.Red;
                return Color.FromArgb("#017CBA");
            }
        }

        public string CurrentInventoryText
        {
            get
            {
                // Calculate current inventory
                double currentInv = Math.Round(Product.CurrentInventory, Config.Round);
                
                // Before save: show actual current inventory (not adjusted by transfer)
                // After save: show the updated inventory (which is already updated by ApplyTransfer)
                // So we always show Product.CurrentInventory, which reflects the actual state
                
                if (Uom != null && !Uom.IsBase)
                    currentInv /= Uom.Conversion;

                return $"Current Inv: {Math.Round(currentInv, 2)}";
            }
        }

        public string UomText => Uom != null ? $"UoM: {Uom.Name}" : string.Empty;
        public bool ShowUom => Uom != null;

        /// <summary>
        /// Notifies that computed properties (like CurrentInventoryText) have changed
        /// </summary>
        public void NotifyInventoryChanged()
        {
            OnPropertyChanged(nameof(CurrentInventoryText));
            OnPropertyChanged(nameof(StartingInventoryText));
        }

        /// <summary>
        /// Notifies that UoM-related properties have changed
        /// </summary>
        public void NotifyUomChanged()
        {
            OnPropertyChanged(nameof(UomText));
            OnPropertyChanged(nameof(ShowUom));
            OnPropertyChanged(nameof(StartingInventoryText));
            OnPropertyChanged(nameof(CurrentInventoryText));
        }

        [RelayCommand]
        private async Task AddQty()
        {
            if (_parent != null)
            {
                await _parent.ShowAddQtyDialog(this);
            }
        }
    }

    public partial class TransferLineDetViewModel : ObservableObject
    {
        private readonly TransferLineViewModel? _parentLine;
        private readonly TransferOnOffPageViewModel? _parent;

        [ObservableProperty] private Product _product = null!;
        [ObservableProperty] private float _qty;
        [ObservableProperty] private UnitOfMeasure? _uom;
        [ObservableProperty] private string _lot = string.Empty;
        [ObservableProperty] private DateTime _lotExp;
        [ObservableProperty] private double _weight;
        [ObservableProperty] private int _id;

        public TransferLineDet TransferLineDet { get; set; } = null!;

        public TransferLineDetViewModel()
        {
        }

        public TransferLineDetViewModel(TransferLineDet det, TransferLineViewModel? parentLine, TransferOnOffPageViewModel? parent = null)
        {
            _parentLine = parentLine;
            _parent = parent;
            Product = det.Product;
            Qty = det.Qty;
            Uom = det.UoM;
            Lot = det.Lot;
            LotExp = det.LotExp;
            Weight = det.Weight;
            Id = det.Id;
            TransferLineDet = det;
        }

        public string QtyText => Qty.ToString();
        public string UomText => Uom != null ? $"UoM: {Uom.Name}" : string.Empty;
        public bool ShowUom => Uom != null;
        public string LotText => Product.UseLot ? $"Lot: {Lot}" : string.Empty;
        public bool ShowLot => Product.UseLot && !string.IsNullOrEmpty(Lot);

        // Notify QtyText when Qty changes
        partial void OnQtyChanged(float value)
        {
            OnPropertyChanged(nameof(QtyText));
        }

        // Notify UomText and ShowUom when Uom changes
        partial void OnUomChanged(UnitOfMeasure? value)
        {
            OnPropertyChanged(nameof(UomText));
            OnPropertyChanged(nameof(ShowUom));
        }

        [RelayCommand]
        private async Task EditQty()
        {
            if (_parent != null && _parentLine != null)
            {
                await _parent.ShowEditQtyDialog(this, _parentLine);
            }
        }
    }
}
