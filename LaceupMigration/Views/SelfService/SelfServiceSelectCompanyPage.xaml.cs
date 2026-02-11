using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceSelectCompanyPage 
    {
        public SelfServiceSelectCompanyPage(SelfServiceSelectCompanyPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override string? GetRouteName() => "selfservice/selectcompany";

        private async void OnCompanySelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is SelfServiceCompany company && BindingContext is SelfServiceSelectCompanyPageViewModel vm)
            {
                await vm.SelectCompanyAsync(company);
            }
        }
    }
}

