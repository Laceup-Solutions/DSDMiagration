
using LaceupMigration.ViewModels;
using System.Linq;

namespace LaceupMigration
{
    
    [QueryProperty(nameof(DownloadData), "downloadData")]
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;

        public string? DownloadData { get; set; }
        
        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;

            // Wire up menu toolbar item
            var menuItem = ToolbarItems.FirstOrDefault();
            if (menuItem != null)
            {
                menuItem.Command = _viewModel.ShowMenuCommand;
            }
            
            if (!string.IsNullOrEmpty(DownloadData) && DownloadData == "1")
                _viewModel.DownloadDataAsync(true);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel?.OnAppearing();
        }
    }
}

