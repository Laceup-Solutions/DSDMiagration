using LaceupMigration.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace LaceupMigration.Views
{
	public partial class CreateDepositPage : LaceupContentPage
	{
		private readonly CreateDepositPageViewModel _viewModel;

		public CreateDepositPage()
		{
			InitializeComponent();
			_viewModel = App.Services.GetRequiredService<CreateDepositPageViewModel>();
			BindingContext = _viewModel;
		}

		protected override string? GetRouteName() => "createdeposit";

		protected override async void OnAppearing()
		{
			base.OnAppearing();
			await _viewModel.InitializeAsync();
		}
	}
}

