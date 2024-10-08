﻿using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using LibVLCSharp;
#if WINDOWS

#else
using LibVLCSharp.MAUI;
#endif
namespace Nyaa_Streamer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitMediaElement()
                .UseMauiCommunityToolkit()
                #if WINDOWS
           
                #else
                .UseLibVLCSharp()
                #endif
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
