using Microsoft.Extensions.Logging;
using ErizosCF.Services;
using ErizosCF.ViewModels;
using ErizosCF.Views;

namespace ErizosCF
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder.Services.AddSingleton<CFService>();
            builder.Services.AddSingleton<EscuelaService>();
            builder.Services.AddTransient<DashBoardViewModel>();
            builder.Services.AddTransient<DashBoardPage>();

            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
