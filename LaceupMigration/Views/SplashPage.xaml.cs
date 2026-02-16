using LaceupMigration.ViewModels;

namespace LaceupMigration
{
	public partial class SplashPage 
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
			
			// When signed out: redirect to login only if NOT in self-service mode.
			// In self-service mode we run Initialize so RouteNextAsync can go straight to self-service shell (e.g. select company) and never show the main app login page.
			if (!Config.SignedIn && !Config.EnableSelfServiceModule)
			{
				_viewModel.RedirectToLoginAsync();
				return;
			}
			
			_viewModel.InitializeCommand.Execute(null);
		}
	}
}

