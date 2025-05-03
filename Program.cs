using System;

// ReSharper disable IdentifierTypo

namespace KyodaiBot;

internal class Program
{
    public static int Main(string[] args)
    {
        var bot = new Bot("7836432946:AAH_Ih50qblFVlsgHLcS6FOCbbcs7YQb1OQ");
        var watchdog = new Watchdog();
        

        Console.WriteLine($"@{bot.Me.Username} is running... Press Escape to terminate");
        watchdog.Start();

        while (Console.ReadKey(true).Key != ConsoleKey.Escape) ;
        watchdog.Stop();
        bot.Cts.Cancel();

        return 0;
    }
}

