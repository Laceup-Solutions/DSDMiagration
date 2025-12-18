using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceCategoriesPage 
    {
        public SelfServiceCategoriesPage(SelfServiceCategoriesPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SelfServiceCategoriesPageViewModel vm)
            {
                vm.OnAppearing();
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (BindingContext is SelfServiceCategoriesPageViewModel vm)
            {
                vm.SearchText = e.NewTextValue;
            }
        }
    }
}

