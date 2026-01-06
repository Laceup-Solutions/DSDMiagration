using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Media;
using Microsoft.Maui.Storage;

namespace LaceupMigration.ViewModels
{
    public partial class NoServicePageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Order? _order;
        private Batch? _batch;
        private Reason? _selectedReason;
        private List<Reason> _reasons = new();
        private bool _initialized;

        public ObservableCollection<NoServiceImageViewModel> Images { get; } = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private string _reasonButtonText = "Select Reason";

        [ObservableProperty]
        private bool _showReasonButton;

        [ObservableProperty]
        private string _reasonText = string.Empty;

        [ObservableProperty]
        private bool _showReasonText;

        [ObservableProperty]
        private bool _showImages = true;

        [ObservableProperty]
        private bool _canDone = true;

        public NoServicePageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
            ShowImages = Config.CaptureImages;
        }

        public async Task InitializeAsync(int orderId)
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

            _batch = Batch.List.FirstOrDefault(x => x.Id == _order.BatchId);
            if (_batch == null)
            {
                _batch = new Batch(_order.Client);
                _batch.Id = _order.BatchId;
                _batch.ClockedIn = DateTime.Now;
                _batch.Save();
            }

            _initialized = true;
            LoadOrderData();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            LoadOrderData();
            await Task.CompletedTask;
        }

        private void LoadOrderData()
        {
            if (_order == null)
                return;

            ClientName = _order.Client?.ClientName ?? "Unknown Client";

            _reasons = Reason.GetReasonsByType(ReasonType.No_Service);

            if (_reasons.Count == 0)
            {
                ShowReasonText = true;
                ShowReasonButton = false;
                ReasonText = _order.Comments ?? string.Empty;
            }
            else
            {
                ShowReasonText = false;
                ShowReasonButton = true;
                _selectedReason = _reasons.FirstOrDefault(x => x.Id == _order.ReasonId);
                ReasonButtonText = _selectedReason != null ? _selectedReason.Description : "Select Reason";
            }

            // Load images
            Images.Clear();
            foreach (var imageId in _order.ImageList)
            {
                var imagePath = Path.Combine(Config.OrderImageStorePath, imageId);
                if (File.Exists(imagePath))
                {
                    Images.Add(new NoServiceImageViewModel { ImagePath = imagePath });
                }
            }

            UpdateCanDone();
        }

        private void UpdateCanDone()
        {
            if (Config.ImageInNoServiceMandatory)
            {
                CanDone = Images.Count > 0 && (!ShowReasonButton || _selectedReason != null) && (!ShowReasonText || !string.IsNullOrWhiteSpace(ReasonText));
            }
            else
            {
                CanDone = (!ShowReasonButton || _selectedReason != null) && (!ShowReasonText || !string.IsNullOrWhiteSpace(ReasonText));
            }
        }

        [RelayCommand]
        private async Task SelectReasonAsync()
        {
            if (_reasons.Count == 0)
                return;

            var reasonNames = _reasons.Select(x => x.Description).ToArray();
            var selectedIndex = await _dialogService.ShowSelectionAsync("Select Reason", reasonNames);
            
            if (selectedIndex >= 0 && selectedIndex < _reasons.Count)
            {
                _selectedReason = _reasons[selectedIndex];
                ReasonButtonText = _selectedReason.Description;
                UpdateCanDone();
            }
        }

        [RelayCommand]
        private async Task TakePhotoAsync()
        {
            try
            {
                if (!MediaPicker.IsCaptureSupported)
                {
                    await _dialogService.ShowAlertAsync("Camera is not available on this device.", "Error");
                    return;
                }

                var photo = await MediaPicker.CapturePhotoAsync();
                if (photo == null)
                    return;

                // Save photo
                var imageId = Guid.NewGuid().ToString("N");
                var filePath = Path.Combine(Config.OrderImageStorePath, imageId);

                // Ensure directory exists
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (var sourceStream = await photo.OpenReadAsync())
                using (var fileStream = File.Create(filePath))
                {
                    await sourceStream.CopyToAsync(fileStream);
                }

                if (_order != null)
                {
                    _order.ImageList.Add(imageId);
                    _order.Save();
                }

                Images.Add(new NoServiceImageViewModel { ImagePath = filePath });
                UpdateCanDone();
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error taking photo: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error taking photo.", "Error");
            }
        }

        [RelayCommand]
        private async Task DoneAsync()
        {
            if (_order == null)
                return;

            int reasonId = 0;
            string reasonComment = string.Empty;

            if (_reasons.Count == 0)
            {
                if (string.IsNullOrWhiteSpace(ReasonText))
                {
                    await _dialogService.ShowAlertAsync("Please enter the no service reason.", "Alert");
                    return;
                }
                reasonComment = ReasonText;
            }
            else
            {
                if (_selectedReason == null)
                {
                    await _dialogService.ShowAlertAsync("Please select the no service reason.", "Alert");
                    return;
                }
                reasonId = _selectedReason.Id;
                reasonComment = _selectedReason.Description;
            }

            if (Config.ImageInNoServiceMandatory && Images.Count == 0)
            {
                await _dialogService.ShowAlertAsync("At least one image is required for no service orders.", "Alert");
                return;
            }

            _order.PrintedOrderId = InvoiceIdProvider.CurrentProvider().GetId(_order);
            _order.ReasonId = reasonId;
            _order.Comments = reasonComment;
            _order.Finished = true;
            _order.EndDate = DateTime.Now;
            _order.Save();

            if (_batch != null)
            {
                _batch.Status = BatchStatus.Locked;
                _batch.ClockedOut = DateTime.Now;
                _batch.Save();
            }

            if (Session.session != null)
                Session.session.AddDetailFromOrder(_order);

            UpdateRoute(true);
            BackgroundDataSync.SyncFinalizedOrders();

            await Shell.Current.GoToAsync("..");
        }

        private void UpdateRoute(bool close)
        {
            if (!Config.CloseRouteInPresale)
                return;

            if (_order == null)
                return;

            var stop = RouteEx.Routes.FirstOrDefault(x => 
                x.Date.Date == DateTime.Today && 
                x.Client != null && 
                x.Client.ClientId == _order.Client.ClientId);

            if (stop != null)
            {
                if (close)
                {
                    stop.Closed = true;
                    stop.When = DateTime.Now;
                    stop.Latitude = DataAccess.LastLatitude;
                    stop.Longitude = DataAccess.LastLongitude;
                }

                if (_order.UniqueId != null)
                    stop.AddOrderToStop(_order.UniqueId);

                RouteEx.Save();
            }
        }
    }

    public partial class NoServiceImageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _imagePath = string.Empty;
    }
}

