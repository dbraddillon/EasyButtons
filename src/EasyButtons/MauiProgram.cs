using EasyButtons.Data;
using EasyButtons.Pages;
using EasyButtons.Repositories;
using EasyButtons.Services;
using EasyButtons.ViewModels;
using Microsoft.Extensions.Logging;

namespace EasyButtons;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var s = builder.Services;
        s.AddSingleton<DatabaseContext>();
        s.AddSingleton<EasyButtonRepository>();
        s.AddSingleton<ProService>();
        s.AddSingleton<MainViewModel>();
        s.AddSingleton<MainPage>();
        s.AddTransient<EditButtonViewModel>();
        s.AddTransient<EditButtonPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
