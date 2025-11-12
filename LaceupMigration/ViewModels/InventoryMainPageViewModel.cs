using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class InventoryMainPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;

        [ObservableProperty] private bool _showAcceptLoad;
        [ObservableProperty] private bool _acceptLoadEnabled;
        [ObservableProperty] private bool _showPendingLoad;
        [ObservableProperty] private bool _showLoadOrder;
        [ObservableProperty] private bool _showParLevel;
        [ObservableProperty] private bool _showTransfers;
        [ObservableProperty] private string _viewPrintText = "View/Print Inventory";

        public InventoryMainPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task OnAppearingAsync()
        {
            BackgroundDataSync.UpdateInvValuesFromOrders();

            ShowAcceptLoad = Config.Delivery;
            AcceptLoadEnabled = Config.SyncLoadOnDemand || Config.NewSyncLoadOnDemand || DataAccess.PendingLoadToAccept;
            ShowPendingLoad = DataAccess.PendingLoadToAccept;
            ShowLoadOrder = Config.LoadRequest;
            ShowParLevel = Config.SetParLevel;
            ShowTransfers = !Config.HideTransfers;
            ViewPrintText = Config.PrinterAvailable ? "View/Print Inventory" : "View Inventory";
        }

        [RelayCommand]
        private async Task ViewPrintInventory()
        {
            await Shell.Current.GoToAsync("viewprintinventory");
        }

        [RelayCommand]
        private async Task CheckInventory()
        {
            await Shell.Current.GoToAsync("checkinventory");
        }

        [RelayCommand]
        private async Task AcceptLoad()
        {
            await Shell.Current.GoToAsync("acceptload");
        }

        [RelayCommand]
        private async Task TransferOn()
        {
            await Shell.Current.GoToAsync("transferonoff?action=transferOn");
        }

        [RelayCommand]
        private async Task TransferOff()
        {
            await Shell.Current.GoToAsync("transferonoff?action=transferOff");
        }

        [RelayCommand]
        private async Task ViewLoadOrder()
        {
            await Shell.Current.GoToAsync("viewloadorder");
        }

        [RelayCommand]
        private async Task SetParLevel()
        {
            await Shell.Current.GoToAsync("setparlevel");
        }

        [RelayCommand]
        private async Task CycleCount()
        {
            await Shell.Current.GoToAsync("cyclecount");
        }

        [RelayCommand]
        private async Task InventorySummary()
        {
            await Shell.Current.GoToAsync("inventorysummary");
        }
    }
}
