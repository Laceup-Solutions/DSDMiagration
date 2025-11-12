using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public enum TransferAction
    {
        On,
        Off
    }

    public partial class TransferOnOffPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        private TransferAction _transferAction;

        [ObservableProperty] private ObservableCollection<InventoryLineViewModel> _inventoryLines = new();
        [ObservableProperty] private string _totalText = string.Empty;
        [ObservableProperty] private bool _readOnly;
        [ObservableProperty] private string _title = "Transfer";

        public TransferOnOffPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task InitializeAsync(string action)
        {
            _transferAction = action == "transferOn" ? TransferAction.On : TransferAction.Off;
            Title = _transferAction == TransferAction.On ? "Transfer On" : "Transfer Off";
            await LoadInventoryAsync();
        }

        private async Task LoadInventoryAsync()
        {
            await Task.Run(() =>
            {
                var productList = new System.Collections.Generic.List<InventoryLine>();

                foreach (var item in Product.Products.Where(x => x.CategoryId > 0 && x.ProductType == ProductType.Inventory))
                {
                    productList.Add(new InventoryLine
                    {
                        Product = item,
                        Real = 0,
                        Starting = item.CurrentInventory
                    });
                }

                var sorted = SortDetails.SortedDetails(productList).ToList();

                System.Collections.Generic.List<InventoryLine> source;
                if (_transferAction == TransferAction.Off)
                {
                    source = sorted.Where(x => x.Starting > 0).ToList();
                }
                else
                {
                    source = sorted;
                }

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    InventoryLines.Clear();
                    foreach (var line in source)
                    {
                        InventoryLines.Add(new InventoryLineViewModel(line, null));
                    }
                    UpdateTotal();
                });
            });
        }

        private void UpdateTotal()
        {
            var total = InventoryLines.Sum(x => x.Real);
            TotalText = $"Total: {total}";
        }

        [RelayCommand]
        private async Task Save()
        {
            // TODO: Implement save logic
            await _dialogService.ShowAlertAsync("Save functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task Print()
        {
            // TODO: Implement print functionality
            await _dialogService.ShowAlertAsync("Print functionality to be implemented.", "Info", "OK");
        }

        [RelayCommand]
        private async Task Search()
        {
            var searchText = await _dialogService.ShowPromptAsync("Search", "Enter product name", "OK", "Cancel", "", -1, "");
            if (!string.IsNullOrEmpty(searchText))
            {
                var upper = searchText.ToUpper();
                var filtered = InventoryLines.Where(x => x.ProductName.ToUpper().Contains(upper)).ToList();
                
                if (filtered.Count > 0)
                {
                    InventoryLines.Clear();
                    foreach (var item in filtered)
                    {
                        InventoryLines.Add(item);
                    }
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Product not found.", "Alert", "OK");
                }
            }
        }
    }
}

