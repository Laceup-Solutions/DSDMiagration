using LaceupMigration.ViewModels.SelfService;
using System.Collections.Generic;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceOffersPage : IQueryAttributable
    {
        public SelfServiceOffersPage(SelfServiceOffersPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override string? GetRouteName() => "offers";

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (BindingContext is SelfServiceOffersPageViewModel vm)
                vm.ApplyQueryAttributes(query);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SelfServiceOffersPageViewModel vm)
            {
                vm.OnAppearing();
            }
        }
    }
}
