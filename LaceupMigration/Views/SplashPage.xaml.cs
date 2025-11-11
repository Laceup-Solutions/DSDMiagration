using LaceupMigration.ViewModels;

namespace LaceupMigration
{
	public partial class SplashPage : ContentPage
	{
		private readonly SplashPageViewModel _viewModel;

		public SplashPage(SplashPageViewModel viewModel)
		{
			InitializeComponent();
			BindingContext = _viewModel = viewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_viewModel.InitializeCommand.Execute(null);
		}
	}
}

