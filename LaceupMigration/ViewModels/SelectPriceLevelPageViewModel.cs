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
    public partial class SelectPriceLevelPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<PriceLevel> _allPriceLevels = new();

        [ObservableProperty] private ObservableCollection<PriceLevelViewModel> _priceLevels = new();
        [ObservableProperty] private string _searchText = string.Empty;

        public SelectPriceLevelPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                _allPriceLevels = PriceLevel.List.ToList();
                FilterPriceLevels(string.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading price levels: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        public void FilterPriceLevels(string searchText)
        {
            PriceLevels.Clear();

            var filtered = string.IsNullOrWhiteSpace(searchText)
                ? _allPriceLevels
                : _allPriceLevels.Where(x => x.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true).ToList();

            foreach (var priceLevel in filtered)
            {
                PriceLevels.Add(new PriceLevelViewModel
                {
                    Id = priceLevel.Id,
                    Name = priceLevel.Name ?? $"Price Level {priceLevel.Id}"
                });
            }
        }

        [RelayCommand]
        private async Task SelectPriceLevel(PriceLevelViewModel priceLevel)
        {
            if (priceLevel != null)
            {
                var result = new Dictionary<string, object>
                {
                    { "priceLevelId", priceLevel.Id },
                    { "priceLevelName", priceLevel.Name }
                };

                await Shell.Current.GoToAsync("..", result);
            }
        }
    }

    public partial class PriceLevelViewModel : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _name = string.Empty;
    }
}

