namespace KyodaiBot;

internal class Program
{
    public static async Task<int> Main(string[] args) // ← Обязательно async Task<int>
    {
        string? explicitTgToken = null;
        string? explicitCocToken = null;

        string actualTgToken = explicitTgToken ?? Environment.GetEnvironmentVariable("TELEGRAM_API_TOKEN")!;
        string actualCocToken = explicitCocToken ?? Environment.GetEnvironmentVariable("CLASH_API_TOKEN")!;

        var bot = new Bot(actualTgToken, actualCocToken);
        var watchdog = new Watchdog();

        Console.WriteLine($"@{bot.Me.Username} is running... Press Ctrl+C to terminate");

        watchdog.Start();

        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            bot.Cts.Cancel();
        };

        try
        {
            await Task.Delay(-1, bot.Cts.Token); // Block indefinitely until cancellation
        }
        catch (TaskCanceledException)
        { } // Ignore

        watchdog.Stop();
        return 0;
    }
}