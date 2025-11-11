using CommunityToolkit.Maui;
using LaceupMigration.Controls;
using MauiIcons.Material.Outlined;
using Microsoft.Extensions.Logging;

namespace LaceupMigration
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
				.UseMauiCommunityToolkit()
				.UseMaterialOutlinedMauiIcons()
                .ConfigureFonts(fonts =>
                {
					fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
					fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("roboto-regular.ttf", "Roboto");
                    fonts.AddFont("roboto-medium.ttf", "Roboto-Medium");
                    fonts.AddFont("roboto-bold.ttf", "Roboto-Bold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<IInterfaceHelper, InterfaceHelper>();
            builder.Services.AddSingleton<IScannerService, ScannerService>();

            // Register DialogService - use same instance for both interface and concrete type
            var dialogService = new DialogService();
            builder.Services.AddSingleton<IDialogService>(dialogService);
            builder.Services.AddSingleton(dialogService);

			builder.Services.AddSingleton<Services.ILaceupAppService, Services.LaceupAppService>();

			// Register AppShell
			builder.Services.AddSingleton<AppShell>();

			// Views + ViewModels
			builder.Services.AddTransient<SplashPage>();
			builder.Services.AddTransient<ViewModels.SplashPageViewModel>();
			builder.Services.AddTransient<LoginConfigPage>();
			builder.Services.AddTransient<ViewModels.LoginConfigPageViewModel>();
			builder.Services.AddTransient<TermsAndConditionsPage>();
			builder.Services.AddTransient<ViewModels.TermsAndConditionsPageViewModel>();
			builder.Services.AddTransient<MainPage>();
			builder.Services.AddSingleton<ViewModels.MainPageViewModel>();
			builder.Services.AddTransient<ClientsPage>();
			builder.Services.AddTransient<ViewModels.ClientsPageViewModel>();
			builder.Services.AddTransient<InvoicesPage>();
			builder.Services.AddTransient<ViewModels.InvoicesPageViewModel>();
			builder.Services.AddTransient<OrdersPage>();
			builder.Services.AddTransient<ViewModels.OrdersPageViewModel>();
			builder.Services.AddTransient<PaymentsPage>();
			builder.Services.AddTransient<ViewModels.PaymentsPageViewModel>();

            return builder.Build();
        }
    }
}
