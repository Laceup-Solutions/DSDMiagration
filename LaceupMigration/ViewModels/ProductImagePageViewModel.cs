using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class ProductImagePageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _imagePath = string.Empty;

        [ObservableProperty]
        private bool _hasImage = false;

        [ObservableProperty]
        private bool _showNoImage = false;

        public async Task InitializeAsync(int? productId = null, string? imagePath = null)
        {
            string path = string.Empty;
            
            if (!string.IsNullOrEmpty(imagePath))
            {
                path = imagePath;
            }
            else if (productId.HasValue)
            {
                path = ProductImage.GetProductImage(productId.Value);
            }
            
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                ImagePath = path;
                HasImage = true;
                ShowNoImage = false;
            }
            else
            {
                HasImage = false;
                ShowNoImage = true;
            }
            
            await Task.CompletedTask;
        }
    }
}

