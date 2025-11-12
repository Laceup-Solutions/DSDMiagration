using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceSelectCompanyPage : ContentPage
    {
        public SelfServiceSelectCompanyPage(SelfServiceSelectCompanyPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        private async void OnCompanySelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is SelfServiceCompany company && BindingContext is SelfServiceSelectCompanyPageViewModel vm)
            {
                await vm.SelectCompanyAsync(company);
            }
        }
    }
}

