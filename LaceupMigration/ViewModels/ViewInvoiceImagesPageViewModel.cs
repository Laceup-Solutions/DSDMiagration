using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LaceupMigration.Controls;
using LaceupMigration.Services;
using Microsoft.Maui.Storage;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class ViewInvoiceImagesPageViewModel : ObservableObject
    {
        private readonly DialogService _dialogService;
        private readonly ILaceupAppService _appService;
        private string _invoiceNumber = string.Empty;
        private bool _initialized;
        private bool _isLoading;

        public ObservableCollection<InvoiceImageViewModel> Images { get; } = new();

        [ObservableProperty]
        private string _invoiceNumberText = string.Empty;

        [ObservableProperty]
        private bool _showImages;

        [ObservableProperty]
        private bool _showNoImages;

        public ViewInvoiceImagesPageViewModel(DialogService dialogService, ILaceupAppService appService)
        {
            _dialogService = dialogService;
            _appService = appService;
        }

        public async Task InitializeAsync(string invoiceNumber)
        {
            if (_initialized && _invoiceNumber == invoiceNumber)
            {
                return;
            }

            _invoiceNumber = invoiceNumber;
            _initialized = true;
            _isLoading = true;
            
            InvoiceNumberText = $"Images For Invoice: {invoiceNumber}";
            
            try
            {
                await LoadImagesAsync();
            }
            finally
            {
                _isLoading = false;
            }
        }

        public async Task OnAppearingAsync()
        {
            if (!_initialized || _isLoading)
                return;

            await LoadImagesAsync();
        }

        private async Task LoadImagesAsync()
        {
            if (string.IsNullOrEmpty(_invoiceNumber))
                return;

            try
            {
                await _dialogService.ShowLoadingAsync("Getting Invoice Images...");
                
                string folderPath = string.Empty;
                
                await Task.Run(() =>
                {
                    try
                    {
                        folderPath = DataProvider.GetExternalInvoiceImages(_invoiceNumber);
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog(ex);
                    }
                });

                await _dialogService.HideLoadingAsync();

                var imageList = LoadInvoiceImages(folderPath);
                
                Images.Clear();
                foreach (var imagePath in imageList)
                {
                    Images.Add(new InvoiceImageViewModel
                    {
                        ImagePath = imagePath
                    });
                }

                if (Images.Count == 0)
                {
                    ShowNoImages = true;
                    ShowImages = false;
                    await _dialogService.ShowAlertAsync("There is no image associated to this invoice", "Info", "OK");
                }
                else
                {
                    ShowImages = true;
                    ShowNoImages = false;
                }
            }
            catch (Exception ex)
            {
                await _dialogService.HideLoadingAsync();
                Logger.CreateLog($"Error loading invoice images: {ex.Message}");
                await _dialogService.ShowAlertAsync("Error loading invoice images.", "Error");
                ShowNoImages = true;
                ShowImages = false;
            }
        }

        private System.Collections.Generic.List<string> LoadInvoiceImages(string path)
        {
            var imageList = new System.Collections.Generic.List<string>();
            
            if (string.IsNullOrEmpty(path))
                return imageList;

            try
            {
                string tempPathFolder = Config.InvoiceImagesTempStorePath;

                if (Directory.Exists(tempPathFolder))
                    Directory.Delete(tempPathFolder, true);

                Directory.CreateDirectory(tempPathFolder);

                ZipFile.ExtractToDirectory(path, tempPathFolder, true);

                var files = Directory.GetFiles(tempPathFolder);
                foreach (var file in files)
                {
                    imageList.Add(file);
                }
            }
            catch (Exception ex)
            {
                Logger.CreateLog($"Error extracting invoice images: {ex.Message}");
            }
            finally
            {
                if (File.Exists(path))
                {
                    try
                    {
                        File.Delete(path);
                    }
                    catch (Exception ex)
                    {
                        Logger.CreateLog($"Error deleting temp file: {ex.Message}");
                    }
                }
            }

            return imageList;
        }

        [RelayCommand]
        private async Task ViewImageAsync(InvoiceImageViewModel image)
        {
            if (image == null || string.IsNullOrEmpty(image.ImagePath))
                return;

            // Navigate to product image page for full screen view
            await Shell.Current.GoToAsync($"productimage?imagePath={Uri.EscapeDataString(image.ImagePath)}");
        }
    }

    public class InvoiceImageViewModel
    {
        public string ImagePath { get; set; } = string.Empty;
    }
}

