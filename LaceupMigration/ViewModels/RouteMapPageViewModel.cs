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
    public partial class RouteMapPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private ObservableCollection<RouteStopViewModel> _routeStops = new();
        [ObservableProperty] private string _routeInfo = string.Empty;

        public RouteMapPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            try
            {
                // TODO: Load route stops from RouteEx or current route
                // For now, create sample data
                RouteStops.Clear();
                
                // TODO: Get actual route data
                // var route = RouteEx.GetCurrentRoute();
                // foreach (var stop in route.Stops)
                // {
                //     RouteStops.Add(new RouteStopViewModel { ... });
                // }

                RouteInfo = $"{RouteStops.Count} stops in route";
            }
            catch (Exception ex)
            {
                await _dialogService.ShowAlertAsync($"Error loading route map: {ex.Message}", "Error", "OK");
                _appService.TrackError(ex);
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            await OnAppearingAsync();
        }

        [RelayCommand]
        private async Task ShowDirections()
        {
            // TODO: Open map app with directions
            await _dialogService.ShowAlertAsync("Directions functionality would open the device's map application with route directions.", "Info", "OK");
        }
    }

    public partial class RouteStopViewModel : ObservableObject
    {
        [ObservableProperty] private string _clientName = string.Empty;
        [ObservableProperty] private string _address = string.Empty;
        [ObservableProperty] private int _sequence;
        [ObservableProperty] private double _latitude;
        [ObservableProperty] private double _longitude;
    }
}

