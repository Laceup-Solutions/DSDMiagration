
using LaceupMigration.ViewModels;
using System.Linq;

namespace LaceupMigration
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;
        
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
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel?.OnAppearing();
        }
    }
}

