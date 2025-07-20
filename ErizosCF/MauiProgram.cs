using ErizosCF.Services;
using ErizosCF.ViewModels;
using ErizosCF.Views;
using Microsoft.Extensions.Logging;

namespace ErizosCF
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder.Services.AddSingleton<CFService>();
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
