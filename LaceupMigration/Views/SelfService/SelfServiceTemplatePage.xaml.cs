using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceTemplatePage 
    {
        public SelfServiceTemplatePage(SelfServiceTemplatePageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override string? GetRouteName() => "selfservice/template";

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

