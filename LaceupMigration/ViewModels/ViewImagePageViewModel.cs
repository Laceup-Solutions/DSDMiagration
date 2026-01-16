using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace LaceupMigration.ViewModels
{
    public partial class ViewImagePageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _imagePath = string.Empty;

        [ObservableProperty]
        private bool _hasImage = false;

        [ObservableProperty]
        private bool _showNoImage = false;

        public async Task InitializeAsync(string? imagePath)
        {
            // Reset state first
            ImagePath = string.Empty;
            HasImage = false;
            ShowNoImage = false;
            
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
