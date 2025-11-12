using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceTemplatePage : ContentPage
    {
        public SelfServiceTemplatePage(SelfServiceTemplatePageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SelfServiceTemplatePageViewModel vm)
            {
                vm.OnAppearing();
            }
        }
    }
}

