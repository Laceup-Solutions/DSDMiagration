using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceCheckOutPage 
    {
        public SelfServiceCheckOutPage(SelfServiceCheckOutPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override string? GetRouteName() => "selfservice/checkout";

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SelfServiceCheckOutPageViewModel vm)
            {
                vm.OnAppearing();
            }
        }
    }
}

