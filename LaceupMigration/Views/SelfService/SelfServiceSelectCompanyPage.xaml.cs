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
    }
}

