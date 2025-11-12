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
    public partial class SelectRetailPriceLevelPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private List<RetailPriceLevel> _allRetailPriceLevels = new();

        [ObservableProperty] private ObservableCollection<RetailPriceLevelViewModel> _retailPriceLevels = new();
        [ObservableProperty] private string _searchText = string.Empty;

        public SelectRetailPriceLevelPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                _allRetailPriceLevels = RetailPriceLevel.Pricelist.OrderBy(x => x.Name).ToList();
                FilterRetailPriceLevels(string.Empty);
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading retail price levels: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        public void FilterRetailPriceLevels(string searchText)
        {
            RetailPriceLevels.Clear();

            var filtered = string.IsNullOrWhiteSpace(searchText)
                ? _allRetailPriceLevels
                : _allRetailPriceLevels.Where(x => x.Name?.Contains(searchText, StringComparison.OrdinalIgnoreCase) == true).ToList();

            foreach (var priceLevel in filtered)
            {
                RetailPriceLevels.Add(new RetailPriceLevelViewModel
                {
                    Id = priceLevel.Id,
                    Name = priceLevel.Name ?? $"Retail Price Level {priceLevel.Id}"
                });
            }
        }

        [RelayCommand]
        private async Task SelectRetailPriceLevel(RetailPriceLevelViewModel priceLevel)
        {
            if (priceLevel != null)
            {
                var result = new Dictionary<string, object>
                {
                    { "retailPriceLevelId", priceLevel.Id },
                    { "retailPriceLevelName", priceLevel.Name }
                };

                await Shell.Current.GoToAsync("..", result);
            }
        }
    }

    public partial class RetailPriceLevelViewModel : ObservableObject
    {
        [ObservableProperty] private int _id;
        [ObservableProperty] private string _name = string.Empty;
    }
}

