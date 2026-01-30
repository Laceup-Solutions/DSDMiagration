using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Services;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LaceupMigration.Controls;

namespace LaceupMigration.ViewModels
{
    public partial class OrderCapturedImageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _imagePath = string.Empty;

        [ObservableProperty]
        private string _caption = string.Empty;

        public int Index { get; set; }
    }

    public partial class ViewCapturedImagesPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private Order? _order;
        private int? _orderId;
        private bool _initialized;

        public ObservableCollection<OrderCapturedImageViewModel> Images { get; } = new();

        [ObservableProperty]
        private bool _showImages;

        [ObservableProperty]
        private bool _showNoImages;

        [ObservableProperty]
        private string _titleText = "Order Images";

        public ViewCapturedImagesPageViewModel(DialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public void SetNavigationQuery(int? orderId)
        {
            _orderId = orderId;
        }

        public async Task InitializeAsync(int? orderId = null, bool fromSelfService = false)
        {
            var oid = orderId ?? _orderId;
            if (!oid.HasValue)
                return;

            if (_initialized && _order?.OrderId == oid.Value)
            {
                await RefreshImagesAsync();
                return;
            }

            _order = Order.Orders.FirstOrDefault(x => x.OrderId == oid.Value);
            if (_order == null)
            {
                await _dialogService.ShowAlertAsync("Order not found.", "Error", "OK");
                await Shell.Current.GoToAsync("..");
                return;
            }

            _initialized = true;
            TitleText = "Order Images";
            await RefreshImagesAsync();
        }

        private Task RefreshImagesAsync()
        {
            Images.Clear();
            if (_order?.ImageList == null)
            {
                ShowImages = false;
                ShowNoImages = true;
                return Task.CompletedTask;
            }

            int offset = 1;
            foreach (var imageId in _order.ImageList)
            {
                var path = Path.Combine(Config.OrderImageStorePath, imageId);
                if (File.Exists(path))
                {
                    Images.Add(new OrderCapturedImageViewModel
                    {
                        ImagePath = path,
                        Caption = $"Capture # {offset}",
                        Index = offset - 1
                    });
                }
                offset++;
            }

            ShowImages = Images.Count > 0;
            ShowNoImages = Images.Count == 0;
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task DoneAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task TakePhotoAsync()
        {
            if (_order == null)
                return;

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

                if (!Directory.Exists(Config.OrderImageStorePath))
                    Directory.CreateDirectory(Config.OrderImageStorePath);

                var imageId = Guid.NewGuid().ToString("N");
                var filePath = Path.Combine(Config.OrderImageStorePath, imageId);

                using (var sourceStream = await photo.OpenReadAsync())
                using (var fileStream = File.Create(filePath))
                {
                    await sourceStream.CopyToAsync(fileStream);
                }

                _order.ImageList.Add(imageId);
                _order.Save();

                await RefreshImagesAsync();

                try
                {
                    if (File.Exists(photo.FullPath))
                        File.Delete(photo.FullPath);
                }
                catch { }
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error taking photo: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error taking photo.", "Error");
            }
        }

        [RelayCommand]
        private async Task ViewImageAsync(OrderCapturedImageViewModel? image)
        {
            if (image == null || string.IsNullOrEmpty(image.ImagePath) || !File.Exists(image.ImagePath))
                return;

            await Shell.Current.GoToAsync($"viewimage?imagePath={Uri.EscapeDataString(image.ImagePath)}");
        }

        [RelayCommand]
        private async Task DeleteImageAsync(OrderCapturedImageViewModel? image)
        {
            if (image == null || _order == null)
                return;

            var confirmed = await _dialogService.ShowConfirmAsync(
                "Are you sure you want to delete this image?",
                "Warning",
                "Yes",
                "No");

            if (!confirmed)
                return;

            var index = image.Index;
            if (index >= 0 && index < _order.ImageList.Count)
            {
                _order.ImageList.RemoveAt(index);
                _order.Save();
                await RefreshImagesAsync();
            }
        }
    }
}
