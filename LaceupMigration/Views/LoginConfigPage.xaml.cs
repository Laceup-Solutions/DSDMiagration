using LaceupMigration.ViewModels;

namespace LaceupMigration
{
	public partial class LoginConfigPage 
	{
		public LoginConfigPage(LoginConfigPageViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = viewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			var model = BindingContext as LoginConfigPageViewModel;
			model?.OnAppearing();
		}

		private void OnTruckTextChanged(object? sender, TextChangedEventArgs e)
		{
			var viewModel = BindingContext as LoginConfigPageViewModel;
			viewModel?.FilterSuggestions(e.NewTextValue ?? string.Empty);
		}

		private void OnSuggestionTapped(object? sender, TappedEventArgs e)
		{
			if (e.Parameter is SalesmanTruckDTO item)
			{
				var viewModel = BindingContext as LoginConfigPageViewModel;
				viewModel?.SelectSuggestion(item);
			}
		}
	}
}

