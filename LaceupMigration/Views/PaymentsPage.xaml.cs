using LaceupMigration.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LaceupMigration
{
	public partial class PaymentsPage : ContentPage
	{
		private readonly PaymentsPageViewModel _viewModel;
		private readonly MainPageViewModel _mainViewModel;

		public PaymentsPage()
		{
			InitializeComponent();

			_viewModel = App.Services.GetRequiredService<PaymentsPageViewModel>();
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
				Title = "Payments";
			
			await _viewModel.OnAppearingAsync();
		}

		private void SearchBar_SearchButtonPressed(object sender, EventArgs e)
		{
			if (sender is SearchBar searchBar)
			{
				searchBar.Unfocus();
			}
		}

		private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (sender is CheckBox checkBox && _viewModel != null)
			{
				_viewModel.SelectAllCommand.Execute(null);
			}
		}
	}
}
