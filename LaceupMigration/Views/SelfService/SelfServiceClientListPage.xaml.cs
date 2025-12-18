using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceClientListPage 
    {
        public SelfServiceClientListPage(SelfServiceClientListPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SelfServiceClientListPageViewModel vm)
            {
                vm.OnAppearing();
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (BindingContext is SelfServiceClientListPageViewModel vm)
            {
                vm.SearchText = e.NewTextValue;
            }
        }

        private async void OnClientSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ClientItemViewModel clientItem && BindingContext is SelfServiceClientListPageViewModel vm)
            {
                await vm.SelectClientAsync(clientItem);
            }
        }
    }
}

