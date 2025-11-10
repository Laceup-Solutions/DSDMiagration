using LaceupMigration.Controls;
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
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("roboto-regular.ttf", "Roboto");
                    fonts.AddFont("roboto-medium.ttf", "Roboto-Medium");
                    fonts.AddFont("roboto-bold.ttf", "Roboto-Bold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            builder.Services.AddSingleton<IInterfaceHelper, InterfaceHelper>();
            builder.Services.AddSingleton<IScannerService, ScannerService>();
            builder.Services.AddSingleton<IDialogService, DialogService>();

            return builder.Build();
        }
    }
}
