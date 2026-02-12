using System.Linq;
using LaceupMigration.ViewModels.SelfService;

namespace LaceupMigration.Views.SelfService
{
    public partial class SelfServiceClientListPage 
    {
        public SelfServiceClientListPage(SelfServiceClientListPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            UseCustomMenu = true; // Single Menu toolbar item only (Sync Data, Advanced Options, Sign Out when 1 client)
        }

        protected override string? GetRouteName() => "selfservice/clientlist";

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (BindingContext is SelfServiceClientListPageViewModel vm)
            {
                vm.OnAppearing();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            var collectionView = this.FindByName<CollectionView>("ClientCollectionView");
            if (collectionView != null)
            {
                collectionView.SelectedItem = null;
            }
        }

        private async void OnClientSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ClientItemViewModel item && BindingContext is SelfServiceClientListPageViewModel vm)
            {
                await vm.HandleClientSelectionAsync(item);
            }
            if (sender is CollectionView collectionView)
            {
                collectionView.SelectedItem = null;
            }
        }
    }
}

