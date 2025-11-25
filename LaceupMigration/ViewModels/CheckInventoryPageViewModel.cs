using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class CheckInventoryPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private ObservableCollection<CheckInventoryLineViewModel> _inventoryLines = new();
        [ObservableProperty] private bool _hasChanges;
        [ObservableProperty] private bool _isPrinted;
        [ObservableProperty] private bool _showPrintButton;

        public CheckInventoryPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowPrintButton = Config.PrinterAvailable;
        }

        public async Task OnAppearingAsync()
        {
            await LoadInventoryAsync();
        }

        
        public async Task OnDisappearing()
        {
            if (HasChanges)
            {
                await SaveStateAsync();
            }
        }
        
        private async Task LoadInventoryAsync()
        {
            await Task.Run(() =>
            {
                var lines = Product.Products
                    .Where(x => x.BeginigInventory > 0 && x.CategoryId > 0 && x.ProductType == ProductType.Inventory)
                    .OrderBy(x => x.Name)
                    .Select(x => new InventoryLine
                    {
                        Product = x,
                        Real = 0
                    })
                    .ToList();

                // Load previous data if exists
                bool previousDataUsed = false;
                if (File.Exists(Config.CurrentCheckInventoryFile))
                {
                    using (var reader = new StreamReader(Config.CurrentCheckInventoryFile))
                    {
                        string? line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            var parts = line.Split(',');
                            if (parts.Length >= 2 && int.TryParse(parts[0], out var productId) && float.TryParse(parts[1], out var qty))
                            {
                                var item = lines.FirstOrDefault(x => x.Product.ProductId == productId);
                                if (item != null)
                                {
                                    item.Real = qty;
                                }
                            }
                        }
                        previousDataUsed = true;
                    }
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    InventoryLines.Clear();
                    foreach (var line in lines)
                    {
                        InventoryLines.Add(new CheckInventoryLineViewModel(line, this));
                    }

                    if (previousDataUsed)
                    {
                        _dialogService.ShowAlertAsync("Warning", "Data from previous session loaded.", "OK");
                    }
                });
            });
        }

        public void MarkChanged()
        {
            HasChanges = true;
            IsPrinted = false;
        }

        [RelayCommand]
        private async Task Save()
        {
            _appService.RecordEvent("Save button");
            
            // Match Xamarin SaveClicked logic (lines 135-171)
            if (Config.PrinterAvailable && HasChanges && !IsPrinted)
            {
                var confirmed = await _dialogService.ShowConfirmationAsync("Warning", "No printed report. Continue?", "Yes", "No");
                if (!confirmed)
                    return;

                // Ask for password only if warning was confirmed - match Xamarin lines 144-168
                var password = await _dialogService.ShowPromptAsync("Inventory Password", "Enter password", "OK", "Cancel", "", -1, "", Keyboard.Default);
                if (password == null)
                    return; // User cancelled

                if (string.Compare(password, Config.InventoryPassword, StringComparison.CurrentCultureIgnoreCase) != 0)
                {
                    await _dialogService.ShowAlertAsync("Invalid password.", "Alert", "OK");
                    return;
                }

                // Continue to save - match Xamarin SaveChanges (line 157)
                await SaveChangesAsync();
            }
            else
            {
                // If printer not available or already printed, go directly to password prompt
                // Ask for password - match Xamarin lines 144-168
                var password = await _dialogService.ShowPromptAsync("Inventory Password", "Enter password", "OK", "Cancel", "", -1, "", Keyboard.Default);
                if (password == null)
                    return; // User cancelled

                if (string.Compare(password, Config.InventoryPassword, StringComparison.CurrentCultureIgnoreCase) != 0)
                {
                    await _dialogService.ShowAlertAsync("Invalid password.", "Alert", "OK");
                    return;
                }

                // Continue to save
                await SaveChangesAsync();
            }
        }

        private async Task SaveChangesAsync()
        {
            // Match Xamarin SaveChanges logic (lines 173-188)
            var confirmOverride = await _dialogService.ShowConfirmationAsync("Warning", "Override current inventory?", "Yes", "No");
            if (!confirmOverride)
                return;

            try
            {
                // Update the counted inventory to the real qty - match Xamarin line 180
                foreach (var line in InventoryLines)
                {
                    line.InventoryLine.Product.SetCurrentInventory(line.InventoryLine.Real);
                }

                // Indicate it was saved - match Xamarin line 182
                DataAccess.SaveInventory();
                HasChanges = false;

                // Delete temp file - match Xamarin line 185-186
                if (File.Exists(Config.CurrentCheckInventoryFile))
                    File.Delete(Config.CurrentCheckInventoryFile);

                await _dialogService.ShowAlertAsync("Inventory saved successfully.", "Success", "OK");
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error saving inventory: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task Print()
        {
            // TODO: Implement print functionality
            IsPrinted = true;
            await _dialogService.ShowAlertAsync("Print functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task SetQuantity(CheckInventoryLineViewModel line)
        {
            if (line == null)
                return;

            var qtyText = await _dialogService.ShowPromptAsync("Set Quantity", line.InventoryLine.Product.Name, "Set", "Cancel", line.InventoryLine.Real.ToString(CultureInfo.CurrentCulture), -1, "", Keyboard.Numeric);
            if (!string.IsNullOrEmpty(qtyText) && float.TryParse(qtyText, out var qty))
            {
                line.InventoryLine.Real = qty;
                line.UpdateQtyText();
                MarkChanged();
            }
        }

        [RelayCommand]
        private async Task AddQuantity(CheckInventoryLineViewModel line)
        {
            if (line == null)
                return;

            _appService.RecordEvent("Add Qty button");
            
            // Match Xamarin ShowQtyDialog logic (lines 409-446)
            var qtyText = await _dialogService.ShowPromptAsync("Quantity to Add", "", "Add", "Cancel", "1", -1, "", Keyboard.Numeric);
            if (!string.IsNullOrEmpty(qtyText))
            {
                try
                {
                    // Match Xamarin line 424: use int for Add Qty
                    int qty = Convert.ToInt32(qtyText);

                    // Match Xamarin line 426: add to existing Real
                    line.InventoryLine.Real += qty;
                    
                    // Match Xamarin lines 427-430: update display
                    line.UpdateQtyText();
                    
                    // Match Xamarin line 432: mark as changed
                    MarkChanged();
                }
                catch
                {
                    await _dialogService.ShowAlertAsync("Invalid quantity.", "Alert", "OK");
                }
            }
        }

        private async Task SaveStateAsync()
        {
            await Task.Run(() =>
            {
                try
                {
                    if (File.Exists(Config.CurrentCheckInventoryFile))
                        File.Delete(Config.CurrentCheckInventoryFile);

                    using (var writer = new StreamWriter(Config.CurrentCheckInventoryFile))
                    {
                        foreach (var line in InventoryLines)
                        {
                            if (line.InventoryLine.Real > 0)
                            {
                                writer.WriteLine($"{line.InventoryLine.Product.ProductId},{line.InventoryLine.Real}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.CreateLog(ex);
                }
            });
        }
    }

    public partial class CheckInventoryLineViewModel : ObservableObject
    {
        private readonly CheckInventoryPageViewModel _parent;

        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private string _qtyText = "0";
        [ObservableProperty] private string _onHandText = string.Empty;

        public InventoryLine InventoryLine { get; }

        public CheckInventoryLineViewModel(InventoryLine line, CheckInventoryPageViewModel parent)
        {
            InventoryLine = line;
            _parent = parent;
            ProductName = line.Product.Name;
            OnHandText = $"On Hand: {line.Product.BeginigInventory}";
            UpdateQtyText();
        }

        public void UpdateQtyText()
        {
            QtyText = InventoryLine.Real > 0 ? InventoryLine.Real.ToString(CultureInfo.CurrentCulture) : "0";
        }
    }
}

