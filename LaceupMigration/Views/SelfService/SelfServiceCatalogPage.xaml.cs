using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceCatalogPage : ContentPage
    {
        public SelfServiceCatalogPage(SelfServiceCatalogPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SelfServiceCatalogPageViewModel vm)
            {
                vm.OnAppearing();
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (BindingContext is SelfServiceCatalogPageViewModel vm)
            {
                vm.SearchText = e.NewTextValue;
            }
        }
    }
}

