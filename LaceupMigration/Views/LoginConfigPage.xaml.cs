using LaceupMigration.ViewModels;

namespace LaceupMigration
{
	public partial class LoginConfigPage : ContentPage
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
	}
}

