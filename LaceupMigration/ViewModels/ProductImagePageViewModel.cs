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

        public async Task InitializeAsync(int productId)
        {
            var imagePath = ProductImage.GetProductImage(productId);
            
            if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
            {
                ImagePath = imagePath;
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

