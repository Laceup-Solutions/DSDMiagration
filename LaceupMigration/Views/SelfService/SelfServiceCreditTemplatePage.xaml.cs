using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceCreditTemplatePage 
    {
        public SelfServiceCreditTemplatePage(SelfServiceCreditTemplatePageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SelfServiceCreditTemplatePageViewModel vm)
            {
                vm.OnAppearing();
            }
        }
    }
}

