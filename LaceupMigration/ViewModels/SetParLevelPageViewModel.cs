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
    public partial class SetParLevelPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private ObservableCollection<ParLevelLineViewModel> _parLevelLines = new();
        [ObservableProperty] private DateTime _setDate = DateTime.Now;
        [ObservableProperty] private string _setDateText = string.Empty;
        [ObservableProperty] private bool _readOnly;

        public SetParLevelPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            SetDateText = SetDate.ToShortDateString();
        }

        public async Task OnAppearingAsync()
        {
            await LoadParLevelsAsync();
        }

        private async Task LoadParLevelsAsync()
        {
            await Task.Run(() =>
            {
                var productList = new System.Collections.Generic.List<InventoryLine>();

                foreach (var detail in ParLevel.List)
                {
                    productList.Add(new InventoryLine
                    {
                        Product = detail.Product,
                        Real = detail.Qty
                    });
                }

                foreach (var p in Product.Products.Where(x => x.CategoryId > 0))
                {
                    var existing = productList.FirstOrDefault(x => x.Product.ProductId == p.ProductId);
                    if (existing == null || existing.Real == 0)
                    {
                        productList.Add(new InventoryLine
                        {
                            Product = p,
                            Real = 0,
                            Starting = 0
                        });
                    }
                }

                var sorted = SortDetails.SortedDetails(productList).ToList();

                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ParLevelLines.Clear();
                    foreach (var line in sorted.Where(x => x.Real > 0 || x.Starting == -1))
                    {
                        ParLevelLines.Add(new ParLevelLineViewModel(line, this));
                    }
                });
            });
        }

        [RelayCommand]
        private async Task Save()
        {
            // TODO: Implement save par levels
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
                var filtered = ParLevelLines.Where(x => x.ProductName.ToUpper().Contains(upper)).ToList();
                
                if (filtered.Count > 0)
                {
                    ParLevelLines.Clear();
                    foreach (var item in filtered)
                    {
                        ParLevelLines.Add(item);
                    }
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Product not found.", "Alert", "OK");
                }
            }
        }

        partial void OnSetDateChanged(DateTime value)
        {
            SetDateText = SetDate.ToShortDateString();
        }
    }

    public partial class ParLevelLineViewModel : ObservableObject
    {
        private readonly SetParLevelPageViewModel _parent;

        [ObservableProperty] private string _productName = string.Empty;
        [ObservableProperty] private float _qty;

        public InventoryLine InventoryLine { get; }

        public ParLevelLineViewModel(InventoryLine line, SetParLevelPageViewModel parent)
        {
            InventoryLine = line;
            _parent = parent;
            ProductName = line.Product.Name;
            Qty = line.Real;
        }
    }
}

