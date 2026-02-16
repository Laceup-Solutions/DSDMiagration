using System.Collections.Generic;
using LaceupMigration;
using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceCatalogPage 
    {
        public SelfServiceCatalogPage(SelfServiceCatalogPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            OverrideBaseMenu = true; // No MENU toolbar item on this page; only Checkout
        }

        protected override string? GetRouteName() => "selfservice/catalog";

        protected override List<MenuOption> GetPageSpecificMenuOptions() => new List<MenuOption>();

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SelfServiceCatalogPageViewModel vm)
            {
                vm.OnAppearing();
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            if (BindingContext is SelfServiceCatalogPageViewModel vm)
            {
                vm.SearchText = e.NewTextValue;
            }
        }
    }
}

