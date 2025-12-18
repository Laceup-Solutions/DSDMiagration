using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceCollectPaymentPage 
    {
        public SelfServiceCollectPaymentPage(SelfServiceCollectPaymentPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SelfServiceCollectPaymentPageViewModel vm)
            {
                vm.OnAppearing();
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (BindingContext is SelfServiceCollectPaymentPageViewModel vm)
            {
                vm.SearchText = e.NewTextValue;
            }
        }

        private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (sender is CheckBox checkBox && BindingContext is SelfServiceCollectPaymentPageViewModel vm)
            {
                // Only trigger if this is the "Select All" checkbox (not an item checkbox)
                // The ViewModel's SelectAll method has a guard to prevent loops
                vm.SelectAllCommand.Execute(null);
            }
        }
    }
}

