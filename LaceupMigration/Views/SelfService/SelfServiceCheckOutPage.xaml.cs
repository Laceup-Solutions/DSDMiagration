using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceCheckOutPage : ContentPage
    {
        public SelfServiceCheckOutPage(SelfServiceCheckOutPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

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

