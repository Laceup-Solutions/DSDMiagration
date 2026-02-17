using LaceupMigration.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Dispatching;

namespace LaceupMigration
{
	public partial class ClientsPage : MainTabContentPage
	{
		private readonly ClientsPageViewModel _viewModel;
		private readonly MainPageViewModel _mainViewModel;
		
		public ClientsPageViewModel ViewModel => _viewModel;

		public ClientsPage()
		{
			InitializeComponent();

			_viewModel = App.Services.GetRequiredService<ClientsPageViewModel>();
			_mainViewModel = App.Services.GetRequiredService<MainPageViewModel>();
			BindingContext = _viewModel;

			// Wire up menu toolbar item
			var menuItem = ToolbarItems.FirstOrDefault();
			if (menuItem != null)
			{
				menuItem.Command = _mainViewModel.ShowMenuCommand;
			}
		}

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			
			// [MIGRATION]: Update page title with company name (matches Xamarin UpdateCompanyName)
			// This ensures the company name appears in the header, just like Xamarin Activity.Title
			var company = CompanyInfo.GetMasterCompany();
			if (company != null && !string.IsNullOrEmpty(company.CompanyName))
				Title = company.CompanyName;
			else
				Title = "Customers";
			
			await _viewModel.OnAppearingAsync();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			var collectionView = this.FindByName<CollectionView>("FlatCollectionView");
			if (collectionView != null)
			{
				collectionView.SelectedItem = null;
			}
		}


		private async void OnClientSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.CurrentSelection.FirstOrDefault() is ClientListItemViewModel item)
			{
				await _viewModel.HandleClientSelectionAsync(item);
			}

			if (sender is CollectionView collectionView)
			{
				collectionView.SelectedItem = null;
			}
		}

		private async void OnGroupedClientSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (e.CurrentSelection.FirstOrDefault() is ClientListItemViewModel item)
			{
				await _viewModel.HandleClientSelectionAsync(item);
			}

			if (sender is CollectionView collectionView)
			{
				collectionView.SelectedItem = null;
			}
		}

		private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
		{
			if (sender is SearchBar searchBar)
			{
				searchBar.Unfocus();
			}
		}
	}
}

