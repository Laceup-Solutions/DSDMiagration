using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class ClientImagesPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private Client? _client;
        private bool _initialized;
        private bool _changed = false;

        public ObservableCollection<ClientImageViewModel> Images { get; } = new();

        [ObservableProperty]
        private string _clientName = string.Empty;

        [ObservableProperty]
        private bool _showImages;

        [ObservableProperty]
        private bool _showNoImages;

        public ClientImagesPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task InitializeAsync(int clientId)
        {
            if (_initialized && _client?.ClientId == clientId)
            {
                await RefreshAsync();
                return;
            }

            _client = Client.Clients.FirstOrDefault(x => x.ClientId == clientId);
            if (_client == null)
            {
                await _dialogService.ShowAlertAsync("Client not found.", "Error");
                return;
            }

            _initialized = true;
            await LoadImagesAsync();
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized)
                return;

            await RefreshAsync();
        }

        private async Task RefreshAsync()
        {
            await LoadImagesAsync();
        }

        private async Task LoadImagesAsync()
        {
            if (_client == null)
                return;

            ClientName = _client.ClientName;

            try
            {
                // Check if online
                var current = Connectivity.NetworkAccess;
                bool isOnline = current != NetworkAccess.None;

                if (isOnline)
                {
                    await LoadImagesOnlineAsync();
                }
                else
                {
                    LoadImagesOffline();
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error loading images: {ex.Message}");
                LoadImagesOffline();
            }
        }

        private async Task LoadImagesOnlineAsync()
        {
            if (_client == null)
                return;

            try
            {
                await _dialogService.ShowLoadingAsync("Loading images...");
                _client.LoadClientImages();
                await _dialogService.HideLoadingAsync();

                LoadImagePaths();
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog($"Error loading images online: {ex.Message}");
                LoadImagesOffline();
            }
        }

        private void LoadImagesOffline()
        {
            if (_client == null)
                return;

            try
            {
                var imageMapPath = Path.Combine(Config.ClientPicturesPath, _client.ClientId.ToString(), "ordersImgMap.txt");
                if (!File.Exists(imageMapPath))
                {
                    LoadImagePaths();
                    return;
                }

                var imageMap = new List<Tuple<int, string>>();
                using (var reader = new StreamReader(imageMapPath))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        var parts = line.Split(',');
                        if (parts.Length >= 2 && int.TryParse(parts[0], out var orderId))
                        {
                            imageMap.Add(new Tuple<int, string>(orderId, parts[1]));
                        }
                    }
                }

                _client.ImageList = imageMap.Select(x => x.Item2).ToList();
                LoadImagePaths();
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error loading images offline: {ex.Message}");
                LoadImagePaths();
            }
        }

        private void LoadImagePaths()
        {
            if (_client == null)
                return;

            Images.Clear();

            var clientImageDir = Path.Combine(Config.ClientPicturesPath, _client.ClientId.ToString());
            if (!Directory.Exists(clientImageDir))
            {
                Directory.CreateDirectory(clientImageDir);
            }

            foreach (var imageId in _client.ImageList)
            {
                var imagePath = Path.Combine(clientImageDir, $"{imageId}.png");
                if (File.Exists(imagePath))
                {
                    Images.Add(new ClientImageViewModel { ImagePath = imagePath, ImageId = imageId });
                }
            }

            ShowImages = Images.Count > 0;
            ShowNoImages = Images.Count == 0;
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
                var clientImageDir = Path.Combine(Config.ClientPicturesPath, _client.ClientId.ToString());
                if (!Directory.Exists(clientImageDir))
                {
                    Directory.CreateDirectory(clientImageDir);
                }

                var filePath = Path.Combine(clientImageDir, $"{imageId}.png");

                using (var sourceStream = await photo.OpenReadAsync())
                using (var fileStream = File.Create(filePath))
                {
                    await sourceStream.CopyToAsync(fileStream);
                }

                if (_client != null)
                {
                    _client.ImageList.Add(imageId);
                    _changed = true;
                }

                Images.Add(new ClientImageViewModel { ImagePath = filePath, ImageId = imageId });
                ShowImages = true;
                ShowNoImages = false;
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error taking photo: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error taking photo.", "Error");
            }
        }

        [RelayCommand]
        private async Task ViewImageAsync(ClientImageViewModel image)
        {
            if (image == null || string.IsNullOrEmpty(image.ImagePath))
                return;

            // Show image in a simple alert dialog with image preview
            // For full screen view, we could navigate to a dedicated page, but for now show alert with path
            await _dialogService.ShowAlertAsync($"Image: {Path.GetFileName(image.ImagePath)}", "Image");
        }

        [RelayCommand]
        private async Task DeleteImageAsync(ClientImageViewModel image)
        {
            if (image == null || _client == null)
                return;

            // Check if online - can only delete when online
            var current = Connectivity.NetworkAccess;
            bool isOnline = current != NetworkAccess.None;

            if (!isOnline)
            {
                await _dialogService.ShowAlertAsync("Cannot delete client images without internet connection. Please connect to the internet and try again.", "Alert");
                return;
            }

            var confirmed = await _dialogService.ShowConfirmationAsync("Warning", "Are you sure you want to delete this image?", "Yes", "No");
            if (!confirmed)
                return;

            try
            {
                // Remove from client's image list
                var imageId = image.ImageId;
                if (!string.IsNullOrEmpty(imageId) && _client.ImageList.Contains(imageId))
                {
                    _client.ImageList.Remove(imageId);
                    _changed = true;
                }

                // Remove from UI
                Images.Remove(image);

                // Delete file
                if (File.Exists(image.ImagePath))
                {
                    File.Delete(image.ImagePath);
                }

                ShowImages = Images.Count > 0;
                ShowNoImages = Images.Count == 0;
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error deleting image: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error deleting image.", "Error");
            }
        }

        [RelayCommand]
        private async Task DoneAsync()
        {
            if (_changed && _client != null)
            {
                await SendImagesAsync();
            }
            else
            {
                await Shell.Current.GoToAsync("..");
            }
        }

        private async Task SendImagesAsync()
        {
            if (_client == null)
                return;

            var current = Connectivity.NetworkAccess;
            bool isOnline = current != NetworkAccess.None;

            string sendingString = isOnline ? "Sending..." : "Saving...";
            await _dialogService.ShowLoadingAsync(sendingString);

            try
            {
                await Task.Run(() =>
                {
                    _client.SendClientPictures(isOnline, _client.ClientId);
                });

                await _dialogService.HideLoadingAsync();
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog($"Error sending images: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error sending images.", "Error");
            }
        }
    }

    public partial class ClientImageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _imagePath = string.Empty;

        [ObservableProperty]
        private string _imageId = string.Empty;
    }
}

